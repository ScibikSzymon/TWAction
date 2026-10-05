using TWAction.Application.AttackCommands.DTOs;
using TWAction.Application.AttackCommands.Interfaces;
using TWAction.Application.AttackCommands.Queries;
using TWAction.Application.Common;
using TWAction.Application.Schedules.Interfaces;

namespace TWAction.Application.AttackCommands.Handlers;

/// <summary>
/// Handler for getting a summary of attack commands for a schedule.
/// Returns null (failure) when no commands have been generated yet.
/// </summary>
public sealed class GetAttackCommandsSummaryHandler(
    IScheduleRepository scheduleRepository,
    IAttackCommandRepository attackCommandRepository)
{
    public async Task<Result<AttackCommandsSummaryDto>> Handle(
        GetAttackCommandsSummaryQuery query,
        CancellationToken cancellationToken = default)
    {
        var schedule = await scheduleRepository.GetByIdAsync(query.ScheduleId, cancellationToken);
        if (schedule is null)
        {
            return Result.Failure<AttackCommandsSummaryDto>("Schedule not found.");
        }

        var commands = await attackCommandRepository.GetByScheduleIdAsync(query.ScheduleId, cancellationToken);
        if (commands.Count == 0)
        {
            return Result.Failure<AttackCommandsSummaryDto>("No commands found.");
        }

        var summary = new AttackCommandsSummaryDto
        {
            TotalCount = commands.Count,
            FirstMinDepartureTime = commands.Min(c => c.MinDepartureTime),
            LastMinDepartureTime = commands.Max(c => c.MinDepartureTime),
            CountByType = commands
                .GroupBy(c => c.CommandType)
                .ToDictionary(g => g.Key, g => g.Count()),
            GeneratedAt = commands.Min(c => c.CreatedAt)
        };

        return Result.Success(summary);
    }
}
