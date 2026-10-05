using TWAction.Application.Common;
using TWAction.Application.MainActions.DTOs;
using TWAction.Application.Schedules.Interfaces;
using TWAction.Application.MainActions.Services;
using TWAction.Application.Schedules.Services;
using TWAction.Application.Tribes.Interfaces;
using TWAction.Domain.Schedules;

namespace TWAction.Application.MainActions.Queries;

public sealed record GetPlayerNobleStatsQuery(Guid ScheduleId);

public class GetPlayerNobleStatsHandler(
    IScheduleRepository scheduleRepository,
    ITroopsStateRepository troopsStateRepository,
    TroopsStateCompressionService compressionService,
    TroopsStateValidator validator,
    ITribesService tribesService)
{
    public async Task<Result<IReadOnlyList<PlayerNobleStatsDto>>> Handle(GetPlayerNobleStatsQuery query, CancellationToken cancellationToken = default)
    {
        var schedule = await scheduleRepository.GetByIdAsync(query.ScheduleId, cancellationToken);
        if (schedule is null)
        {
            return Result.Failure<IReadOnlyList<PlayerNobleStatsDto>>($"Schedule with ID '{query.ScheduleId}' not found.");
        }

        var troopsState = await troopsStateRepository.GetByScheduleIdAsync(query.ScheduleId, cancellationToken);
        if (troopsState is null)
        {
            return Result.Failure<IReadOnlyList<PlayerNobleStatsDto>>("Troops state not found for this schedule. Please upload troops data first.");
        }

        // Decompress troops data
        var decompressResult = compressionService.Decompress(troopsState.CompressedData);
        if (decompressResult.IsFailure)
        {
            return Result.Failure<IReadOnlyList<PlayerNobleStatsDto>>(decompressResult.Error);
        }

        // Parse troops data
        var parseResult = validator.ValidateAndParse(decompressResult.Value);
        if (parseResult.IsFailure)
        {
            return Result.Failure<IReadOnlyList<PlayerNobleStatsDto>>(parseResult.Error);
        }

        // Find the column index for nobles dynamically by looking for "Grube" in header
        var nobleColumnIndex = Array.FindIndex(parseResult.Value.Header, 
            col => col.Equals("Grube", StringComparison.OrdinalIgnoreCase) || 
                   col.Equals("Trebuchet", StringComparison.OrdinalIgnoreCase));

        if (nobleColumnIndex == -1)
        {
            return Result.Failure<IReadOnlyList<PlayerNobleStatsDto>>(
                "Could not find nobles column in CSV header. Expected 'Grube' or 'Trebuchet' column.");
        }

        // Get player IDs from tribes API
        var playersDict = await tribesService.GetPlayersAsync(schedule.World, cancellationToken);

        // Group by player and sum nobles
        var playerStats = parseResult.Value.DataRows
            .GroupBy(row => row[0].Trim()) // Group by player name (column 0)
            .Select(group =>
            {
                var playerName = group.Key;
                var totalNobles = group.Sum(row =>
                {
                    if (int.TryParse(row[nobleColumnIndex], out var nobles))
                    {
                        return nobles;
                    }
                    return 0;
                });

                // Find player ID from tribes API
                var playerInfo = playersDict.Values.FirstOrDefault(p =>
                    p.Nick.Equals(playerName, StringComparison.OrdinalIgnoreCase));

                if (playerInfo != null)
                {
                    return new PlayerNobleStatsDto
                    {
                        PlayerId = playerInfo.Id,
                        PlayerName = playerName,
                        TotalNobles = totalNobles
                    };
                }

                return null;
            })
            .Where(stat => stat != null)
            .Cast<PlayerNobleStatsDto>()
            .OrderBy(stat => stat.PlayerName)
            .ToList();

        return Result.Success<IReadOnlyList<PlayerNobleStatsDto>>(playerStats);
    }
}
