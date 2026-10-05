using TWAction.Application.Common;
using TWAction.Application.Schedules.DTOs;
using TWAction.Application.Schedules.Interfaces;
using TWAction.Application.Schedules.Services;
using TWAction.Application.MainActions.Services;
using TWAction.Domain.Schedules;
using TWAction.Domain.Users;
using TWAction.Application.Users.Interfaces;
using TWAction.Application.Interfaces;

namespace TWAction.Application.Schedules.Commands;

public sealed record UploadTroopsStateCommand(Guid ScheduleId, string RawData);

public class UploadTroopsStateHandler(
    IScheduleRepository scheduleRepository,
    ITroopsStateRepository troopsStateRepository,
    ITroopsUploadRepository uploads,
    IUserRepository users,
    IUserQuotaGuardFactory quotaGuards,
    ISubscriptionPlanLimitsRepository planLimitsRepository,
    ICurrentUserAccessor currentUser,
    TroopsStateValidator validator,
    TroopsStateCompressionService compressionService,
    TroopsStateStatsExtractor statsExtractor)
{
    public async Task<Result<TroopsStateDto>> Handle(UploadTroopsStateCommand command, CancellationToken cancellationToken = default)
    {
        var schedule = await scheduleRepository.GetByIdAsync(command.ScheduleId, cancellationToken);
        if (schedule is null)
        {
            return Result.Failure<TroopsStateDto>($"Schedule with ID '{command.ScheduleId}' not found.");
        }

        if (!currentUser.TryGetUserId(out var userId) || (!currentUser.IsAdmin && schedule.UserGuid != userId))
        {
            return Result.Failure<TroopsStateDto>("Schedule not found for specified user.");
        }

        await using var quota = await quotaGuards.AcquireAsync(schedule.UserGuid, cancellationToken);
        var owner = await users.GetByIdAsync(schedule.UserGuid, cancellationToken);
        if (owner is null)
        {
            return Result.Failure<TroopsStateDto>("User not found.");
        }

        var now = DateTimeOffset.UtcNow;
        var planLimits = await planLimitsRepository.GetAsync(
            owner.SubscriptionTier,
            cancellationToken);
        var uploadWindow = UserLimits.UploadWindow(planLimits);
        var limit = UserLimits.TroopsUploads(owner, planLimits);
        if (limit.HasValue &&
            (await uploads.ListRecentAsync(schedule.Id, now - uploadWindow, cancellationToken)).Count >= limit.Value)
        {
            return Result.Failure<TroopsStateDto>(
                $"Troops upload limit reached ({limit.Value} per {planLimits.TroopsUploadWindowHours} hours).");
        }

        var parseResult = validator.ValidateAndParse(command.RawData);
        if (parseResult.IsFailure)
        {
            return Result.Failure<TroopsStateDto>(parseResult.Error);
        }

        var stats = statsExtractor.Extract(parseResult.Value);
        var compressedData = compressionService.Compress(command.RawData);
        var existingTroopsState = await troopsStateRepository.GetByScheduleIdAsync(command.ScheduleId, cancellationToken);

        TroopsStateEntity troopsState;
        if (existingTroopsState is not null)
        {
            // Update existing
            existingTroopsState.CompressedData = compressedData;
            existingTroopsState.UpdatedAt = now;
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
                CreatedAt = now,
                UpdatedAt = now
            };
            troopsState = await troopsStateRepository.CreateAsync(troopsState, cancellationToken);
        }

        await uploads.RecordAsync(new TroopsUploadEntity
        {
            Id = Guid.NewGuid(), UserId = schedule.UserGuid, ScheduleId = schedule.Id, UploadedAt = now
        }, cancellationToken);
        await quota.CommitAsync(cancellationToken);

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

