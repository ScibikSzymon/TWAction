using TWAction.Application.Common;
using TWAction.Application.Schedules.DTOs;
using TWAction.Application.Schedules.Interfaces;
using TWAction.Application.Schedules.Services;
using TWAction.Domain.Schedules;

namespace TWAction.Application.Schedules.Commands;

public sealed record UploadTroopsStateCommand(Guid ScheduleId, string RawData);

public class UploadTroopsStateHandler(
    IScheduleRepository scheduleRepository,
    ITroopsStateRepository troopsStateRepository,
    TroopsStateValidator validator,
    TroopsStateCompressionService compressionService,
    TroopsStateStatsExtractor statsExtractor)
{
    public async Task<Result<TroopsStateDto>> Handle(UploadTroopsStateCommand command, CancellationToken cancellationToken = default)
    {
        // Verify schedule exists
        var schedule = await scheduleRepository.GetByIdAsync(command.ScheduleId, cancellationToken);
        if (schedule is null)
        {
            return Result.Failure<TroopsStateDto>($"Schedule with ID '{command.ScheduleId}' not found.");
        }

        // Validate and parse troops data
        var parseResult = validator.ValidateAndParse(command.RawData);
        if (parseResult.IsFailure)
        {
            return Result.Failure<TroopsStateDto>(parseResult.Error);
        }

        // Extract stats
        var stats = statsExtractor.Extract(parseResult.Value);

        // Compress data
        var compressedData = compressionService.Compress(command.RawData);

        // Check if troops state already exists for this schedule
        var existingTroopsState = await troopsStateRepository.GetByScheduleIdAsync(command.ScheduleId, cancellationToken);

        TroopsStateEntity troopsState;
        if (existingTroopsState is not null)
        {
            // Update existing
            existingTroopsState.CompressedData = compressedData;
            existingTroopsState.UpdatedAt = DateTimeOffset.UtcNow;
            troopsState = await troopsStateRepository.UpdateAsync(existingTroopsState, cancellationToken);
        }
        else
        {
            // Create new
            troopsState = new TroopsStateEntity
            {
                Id = Guid.NewGuid(),
                ScheduleId = command.ScheduleId,
                CompressedData = compressedData,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };
            troopsState = await troopsStateRepository.CreateAsync(troopsState, cancellationToken);
        }

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

