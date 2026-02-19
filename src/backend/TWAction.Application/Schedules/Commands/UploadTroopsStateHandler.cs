using FluentValidation;
using TWAction.Application.Common;
using TWAction.Application.Interfaces;
using TWAction.Application.Schedules.DTOs;
using TWAction.Application.Schedules.Interfaces;
using TWAction.Application.Schedules.Services;
using TWAction.Domain.Schedules;

namespace TWAction.Application.Schedules.Commands;

public sealed record UploadTroopsStateCommand(Guid ScheduleId, string RawData);

public sealed class UploadTroopsStateCommandValidator : AbstractValidator<UploadTroopsStateCommand>
{
    public UploadTroopsStateCommandValidator(
        ICurrentUserAccessor currentUser,
        IScheduleRepository scheduleRepository)
    {
        RuleFor(x => x.ScheduleId)
            .NotEmpty()
            .WithMessage("Schedule ID must not be empty.");

        RuleFor(x => x.RawData)
            .NotEmpty()
            .WithMessage("Raw data must not be empty.");

        RuleFor(x => x.ScheduleId)
            .MustAsync(async (scheduleId, cancellationToken) =>
            {
                var schedule = await scheduleRepository.GetByIdAsync(scheduleId, cancellationToken);
                return schedule is not null;
            })
            .WithMessage(command => $"Schedule with ID '{command.ScheduleId}' not found.")
            .When(command => currentUser.TryGetUserId(out _));

        RuleFor(x => x.ScheduleId)
            .MustAsync(async (scheduleId, cancellationToken) =>
            {
                var schedule = await scheduleRepository.GetByIdAsync(scheduleId, cancellationToken);
                if (schedule is null) return true;
                if (currentUser.IsAdmin) return true;
                currentUser.TryGetUserId(out var userId);
                return schedule.UserGuid == userId;
            })
            .WithMessage("Schedule not found for specified user.")
            .When(command => currentUser.TryGetUserId(out _));
    }
}

public class UploadTroopsStateHandler(
    ITroopsStateRepository troopsStateRepository,
    TroopsStateValidator validator,
    TroopsStateCompressionService compressionService,
    TroopsStateStatsExtractor statsExtractor,
    IValidator<UploadTroopsStateCommand> fluentValidator)
{
    public async Task<Result<TroopsStateDto>> Handle(UploadTroopsStateCommand command, CancellationToken cancellationToken = default)
    {
        var validationFailure = await FluentValidationBefore.ValidateAsync<UploadTroopsStateCommand, TroopsStateDto>(
            fluentValidator, command, cancellationToken);

        if (validationFailure is not null)
        {
            return validationFailure;
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

