using TWAction.Application.AttackCommands.DTOs;
using TWAction.Application.AttackCommands.Interfaces;
using TWAction.Application.Common;
using TWAction.Application.Schedules.Interfaces;
using TWAction.Application.Tribes.Interfaces;
using TWAction.Domain.AttackCommands;

namespace TWAction.Application.AttackCommands.Handlers;

public sealed record SendToPlemionaRozpiskiCommand(Guid ScheduleId, bool ForceOverwrite = false);

public sealed class SendToPlemionaRozpiskiResponse
{
    public required int CommandsSentCount { get; init; }
    public required DateTimeOffset SentAt { get; init; }
}

public class SendToPlemionaRozpiskiHandler(
    IScheduleRepository scheduleRepository,
    IAttackCommandRepository attackCommandRepository,
    ITribesService tribesService,
    IPlemionaRozpiskiApiClient plemionaRozpiskiApiClient)
{
    private const string DateTimeFormat = "yyyy-MM-dd HH:mm:ss";

    public async Task<Result<SendToPlemionaRozpiskiResponse>> Handle(
        SendToPlemionaRozpiskiCommand command,
        CancellationToken cancellationToken = default)
    {
        // 1. Get schedule
        var schedule = await scheduleRepository.GetByIdAsync(command.ScheduleId, cancellationToken);
        if (schedule is null)
        {
            return Result.Failure<SendToPlemionaRozpiskiResponse>(
                $"Schedule with ID '{command.ScheduleId}' not found.");
        }

        // 2. Get attack commands for this schedule
        var attackCommands = await attackCommandRepository.GetByScheduleIdAsync(
            command.ScheduleId, cancellationToken);

        if (attackCommands.Count == 0)
        {
            return Result.Failure<SendToPlemionaRozpiskiResponse>(
                "No attack commands found for this schedule. Generate commands first.");
        }

        // 3. If schedule was already sent and forceOverwrite is false, return error
        if (schedule.SentToPlemionaRozpiskiAt.HasValue && !command.ForceOverwrite)
        {
            return Result.Failure<SendToPlemionaRozpiskiResponse>(
                $"Schedule was already sent at {schedule.SentToPlemionaRozpiskiAt:yyyy-MM-dd HH:mm:ss}. Use forceOverwrite to replace.");
        }

        // 4. If overwriting, delete old commands from plemionarozpiski.pl
        if (schedule.SentToPlemionaRozpiskiAt.HasValue && command.ForceOverwrite)
        {
            var deleteResult = await plemionaRozpiskiApiClient.DeleteCommandsAsync(
                command.ScheduleId.ToString(),
                cancellationToken);

            if (!deleteResult)
            {
                return Result.Failure<SendToPlemionaRozpiskiResponse>(
                    "Failed to delete old commands from plemionarozpiski.pl.");
            }
        }

        // 5. Get player names from tribes service
        var players = await tribesService.GetPlayersAsync(schedule.World, cancellationToken);

        // 6. Build DTOs - sort by MaxDepartureTime (maxTime) and assign command numbers per player
        var sortedCommands = attackCommands.OrderBy(cmd => cmd.MaxDepartureTime).ToList();
        var playerCommandCounters = new Dictionary<int, int>();

        var commandDtos = sortedCommands.Select(cmd =>
        {
            var playerId = cmd.SourcePlayerId;

            if (!playerCommandCounters.ContainsKey(playerId))
                playerCommandCounters[playerId] = 0;

            playerCommandCounters[playerId]++;

            var playerName = players.TryGetValue(playerId, out var playerInfo)
                ? playerInfo.Nick
                : $"Player_{playerId}";

            return new PlemionaRozpiskiCommandDto
            {
                CommandNumberId = playerCommandCounters[playerId],
                Type = cmd.CommandType,
                MinTime = cmd.MinDepartureTime.ToString(DateTimeFormat),
                MaxTime = cmd.MaxDepartureTime.ToString(DateTimeFormat),
                AttackTime = $"{cmd.MinArrivalTime.ToString(DateTimeFormat)}-{cmd.MaxArrivalTime:HH:mm:ss}",
                Source = $"{cmd.SourceX}|{cmd.SourceY}",
                SourceId = cmd.SourceVillageId.ToString(),
                Target = $"{cmd.DestinationX}|{cmd.DestinationY}",
                TargetId = cmd.DestinationVillageId.ToString(),
                PlayerId = playerId.ToString(),
                PlayerName = playerName,
                World = schedule.World.ToString(),
                OperationName = command.ScheduleId.ToString()
            };
        }).ToList();

        // 7. Send to plemionarozpiski.pl
        var sendResult = await plemionaRozpiskiApiClient.SendCommandsAsync(commandDtos, cancellationToken);

        if (!sendResult.Success)
        {
            return Result.Failure<SendToPlemionaRozpiskiResponse>(
                $"Failed to send commands to plemionarozpiski.pl: {sendResult.ErrorMessage}");
        }

        // 8. Update schedule with sent timestamp
        var sentAt = DateTimeOffset.UtcNow;
        schedule.SentToPlemionaRozpiskiAt = sentAt;
        await scheduleRepository.UpdateAsync(schedule, cancellationToken);

        return Result.Success(new SendToPlemionaRozpiskiResponse
        {
            CommandsSentCount = sendResult.CommandsSentCount,
            SentAt = sentAt
        });
    }
}
