using FluentValidation;
using TWAction.Application.Common;
using TWAction.Application.Interfaces;
using TWAction.Application.Mappers;
using TWAction.Application.Schedules.Interfaces;
using TWAction.Application.Settings.DTOs;
using TWAction.Application.Settings.Interfaces;
using TWAction.Domain.Schedules;
using TWAction.Domain.Settings;

namespace TWAction.Application.Settings.Commands;

public sealed record SaveReconnaissanceSettingsCommand(
    Guid ScheduleId,
    DateTimeOffset MinDepartureTime,
    DateTimeOffset MinArrivalTime,
    DateTimeOffset MaxArrivalTime,
    int MinDistanceToFront,
    int MinSpyCount,
    int MaxPopulationInSourceVillage,
    bool SkipNightSendings
);

public sealed class SaveReconnaissanceSettingsCommandValidator : AbstractValidator<SaveReconnaissanceSettingsCommand>
{
    public SaveReconnaissanceSettingsCommandValidator(
        ICurrentUserAccessor currentUser,
        IScheduleRepository scheduleRepository)
    {
        RuleFor(x => x.ScheduleId)
            .NotEmpty()
            .WithMessage("Schedule ID must not be empty.");

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

        RuleFor(x => x.ScheduleId)
            .MustAsync(async (scheduleId, cancellationToken) =>
            {
                var schedule = await scheduleRepository.GetByIdAsync(scheduleId, cancellationToken);
                if (schedule is null) return true;
                return schedule.ScheduleType == ScheduleType.Reconnaissance;
            })
            .WithMessage("Reconnaissance settings can only be set for schedules with type 'Reconnaissance'.")
            .When(command => currentUser.TryGetUserId(out _));

        RuleFor(x => x.MinArrivalTime)
            .Must((command, minArrival) => minArrival > command.MinDepartureTime)
            .WithMessage("MinArrivalTime must be after MinDepartureTime.");

        RuleFor(x => x.MaxArrivalTime)
            .Must((command, maxArrival) => maxArrival > command.MinArrivalTime)
            .WithMessage("MaxArrivalTime must be after MinArrivalTime.");

        RuleFor(x => x.MinSpyCount)
            .GreaterThanOrEqualTo(1)
            .WithMessage("MinSpyCount must be at least 1.");

        RuleFor(x => x.MinDistanceToFront)
            .GreaterThanOrEqualTo(0)
            .WithMessage("MinDistanceToFront cannot be negative.");

        RuleFor(x => x.MaxPopulationInSourceVillage)
            .GreaterThanOrEqualTo(0)
            .WithMessage("MaxPopulationInSourceVillage cannot be negative.");
    }
}

public class SaveReconnaissanceSettingsHandler(
    IReconnaissanceSettingsRepository settingsRepository,
    IValidator<SaveReconnaissanceSettingsCommand> fluentValidator)
{
    public async Task<Result<ReconnaissanceSettingsDto>> Handle(
        SaveReconnaissanceSettingsCommand command,
        CancellationToken cancellationToken = default)
    {
        var validationFailure = await FluentValidationBefore.ValidateAsync<SaveReconnaissanceSettingsCommand, ReconnaissanceSettingsDto>(
            fluentValidator, command, cancellationToken);

        if (validationFailure is not null)
        {
            return validationFailure;
        }

        // Check if settings already exist (upsert)
        var existing = await settingsRepository.GetByScheduleIdAsync(command.ScheduleId, cancellationToken);

        ReconnaissanceSettings settings;

        if (existing is not null)
        {
            // Update existing
            existing.MinDepartureTime = command.MinDepartureTime;
            existing.MinArrivalTime = command.MinArrivalTime;
            existing.MaxArrivalTime = command.MaxArrivalTime;
            existing.MinDistanceToFront = command.MinDistanceToFront;
            existing.MinSpyCount = command.MinSpyCount;
            existing.MaxPopulationInSourceVillage = command.MaxPopulationInSourceVillage;
            existing.SkipNightSendings = command.SkipNightSendings;

            settings = await settingsRepository.UpdateAsync(existing, cancellationToken);
        }
        else
        {
            // Create new
            settings = new ReconnaissanceSettings
            {
                Id = Guid.NewGuid(),
                ScheduleId = command.ScheduleId,
                MinDepartureTime = command.MinDepartureTime,
                MinArrivalTime = command.MinArrivalTime,
                MaxArrivalTime = command.MaxArrivalTime,
                MinDistanceToFront = command.MinDistanceToFront,
                MinSpyCount = command.MinSpyCount,
                MaxPopulationInSourceVillage = command.MaxPopulationInSourceVillage,
                SkipNightSendings = command.SkipNightSendings
            };

            settings = await settingsRepository.CreateAsync(settings, cancellationToken);
        }

        return Result.Success(IReconnaissanceSettingsMapper.ToDto(settings));
    }
}
