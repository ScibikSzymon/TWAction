using TWAction.Application.Common;
using TWAction.Application.Schedules.DTOs;
using TWAction.Application.Schedules.Interfaces;
using TWAction.Application.Schedules.Services;

namespace TWAction.Application.Schedules.Queries;

public sealed record GetTroopsStateQuery(Guid ScheduleId);

public class GetTroopsStateHandler(
    ITroopsStateRepository troopsStateRepository,
    TroopsStateCompressionService compressionService,
    TroopsStateValidator validator,
    TroopsStateStatsExtractor statsExtractor)
{
    public async Task<Result<TroopsStateDto>> Handle(GetTroopsStateQuery query, CancellationToken cancellationToken = default)
    {
        var troopsState = await troopsStateRepository.GetByScheduleIdAsync(query.ScheduleId, cancellationToken);

        if (troopsState is null)
        {
            return Result.Failure<TroopsStateDto>($"Troops state for schedule '{query.ScheduleId}' not found.");
        }

        // Decompress data to extract stats
        var decompressResult = compressionService.Decompress(troopsState.CompressedData);
        if (decompressResult.IsFailure)
        {
            return Result.Failure<TroopsStateDto>($"Failed to decompress troops data: {decompressResult.Error}");
        }

        // Parse to get stats
        var parseResult = validator.ValidateAndParse(decompressResult.Value);
        if (parseResult.IsFailure)
        {
            return Result.Failure<TroopsStateDto>($"Failed to parse troops data: {parseResult.Error}");
        }

        var stats = statsExtractor.Extract(parseResult.Value);

        var dto = new TroopsStateDto
        {
            Id = troopsState.Id,
            ScheduleId = troopsState.ScheduleId,
            VillageCount = stats.VillageCount,
            PlayerCount = stats.PlayerCount,
            CreatedAt = troopsState.CreatedAt,
            UpdatedAt = troopsState.UpdatedAt
        };

        return Result.Success(dto);
    }
}
