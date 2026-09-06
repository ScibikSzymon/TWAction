using TWAction.Application.AttackCommands.DTOs;
using TWAction.Application.AttackCommands.Interfaces;
using TWAction.Application.AttackCommands.Queries;
using TWAction.Application.Common;
using TWAction.Application.Schedules.Interfaces;
using TWAction.Application.Tribes.Interfaces;

namespace TWAction.Application.AttackCommands.Handlers;

/// <summary>
/// Returns statistics for a generated main action:
/// – commands per enemy (destination) player,
/// – commands per source player,
/// – hourly distribution of commands by arrival hour.
/// </summary>
public sealed class GetMainActionStatsHandler(
    IScheduleRepository scheduleRepository,
    IAttackCommandRepository attackCommandRepository,
    ITribesService tribesService)
{
    public async Task<Result<MainActionStatsDto>> Handle(
        GetMainActionStatsQuery query,
        CancellationToken cancellationToken = default)
    {
        var schedule = await scheduleRepository.GetByIdAsync(query.ScheduleId, cancellationToken);
        if (schedule is null)
        {
            return Result.Failure<MainActionStatsDto>("Schedule not found.");
        }

        var commands = await attackCommandRepository.GetByScheduleIdAsync(query.ScheduleId, cancellationToken);
        if (commands.Count == 0)
        {
            return Result.Failure<MainActionStatsDto>("No commands have been generated yet for this schedule.");
        }

        // Resolve player names once from the tribes service (cached internally).
        var players = await tribesService.GetPlayersAsync(schedule.World, cancellationToken);

        string ResolveName(int playerId)
            => players.TryGetValue(playerId, out var info) ? info.Nick : $"Gracz #{playerId}";

        // ── Commands per enemy (destination) player ─────────────────────────
        var commandsPerEnemyPlayer = commands
            .GroupBy(c => c.DestinationPlayerId)
            .Select(g => new CommandsPerPlayerDto
            {
                PlayerId = g.Key,
                PlayerName = ResolveName(g.Key),
                TotalCount = g.Count(),
                CountByType = g
                    .GroupBy(c => c.CommandType)
                    .ToDictionary(t => t.Key, t => t.Count())
            })
            .OrderByDescending(p => p.TotalCount)
            .ToList();

        // ── Commands per source player ───────────────────────────────────────
        var commandsPerSourcePlayer = commands
            .GroupBy(c => c.SourcePlayerId)
            .Select(g => new CommandsPerPlayerDto
            {
                PlayerId = g.Key,
                PlayerName = ResolveName(g.Key),
                TotalCount = g.Count(),
                CountByType = g
                    .GroupBy(c => c.CommandType)
                    .ToDictionary(t => t.Key, t => t.Count())
            })
            .OrderByDescending(p => p.TotalCount)
            .ToList();

        // ── Commands per arrival hour (based on MinArrivalTime) ─────────────
        var commandsPerArrivalHour = commands
            .GroupBy(c => c.MinArrivalTime.Hour)
            .Select(g => new CommandsPerHourDto
            {
                Hour = g.Key,
                TotalCount = g.Count(),
                CountByType = g
                    .GroupBy(c => c.CommandType)
                    .ToDictionary(t => t.Key, t => t.Count())
            })
            .OrderBy(h => h.Hour)
            .ToList();

        // ── Commands per 8-hour departure period (based on MinDepartureTime) ─
        // Slots: 0–8, 8–16, 16–24
        static int SlotStart(int hour) => hour / 8 * 8;

        var commandsPerDeparturePeriod = commands
            .GroupBy(c => (Date: DateOnly.FromDateTime(c.MinDepartureTime.UtcDateTime), Slot: SlotStart(c.MinDepartureTime.UtcDateTime.Hour)))
            .Select(g => new CommandsPerDeparturePeriodDto
            {
                Date = g.Key.Date,
                SlotStart = g.Key.Slot,
                TotalCount = g.Count(),
                CountByType = g
                    .GroupBy(c => c.CommandType)
                    .ToDictionary(t => t.Key, t => t.Count())
            })
            .OrderBy(p => p.Date)
            .ThenBy(p => p.SlotStart)
            .ToList();

        return Result.Success(new MainActionStatsDto
        {
            TotalCommands = commands.Count,
            CommandsPerEnemyPlayer = commandsPerEnemyPlayer,
            CommandsPerSourcePlayer = commandsPerSourcePlayer,
            CommandsPerArrivalHour = commandsPerArrivalHour,
            CommandsPerDeparturePeriod = commandsPerDeparturePeriod,
        });
    }
}
