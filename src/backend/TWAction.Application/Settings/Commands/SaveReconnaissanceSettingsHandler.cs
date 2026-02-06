using TWAction.Application.Common;
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

public class SaveReconnaissanceSettingsHandler(
    IReconnaissanceSettingsRepository settingsRepository,
    IScheduleRepository scheduleRepository)
{
    public async Task<Result<ReconnaissanceSettingsDto>> Handle(
        SaveReconnaissanceSettingsCommand command,
        CancellationToken cancellationToken = default)
    {
        // Validate schedule exists
        var schedule = await scheduleRepository.GetByIdAsync(command.ScheduleId, cancellationToken);
        if (schedule is null)
        {
            return Result.Failure<ReconnaissanceSettingsDto>(
                $"Schedule with ID '{command.ScheduleId}' not found.");
        }

        // Validate schedule type is Reconnaissance
        if (schedule.ScheduleType != ScheduleType.Reconnaissance)
        {
            return Result.Failure<ReconnaissanceSettingsDto>(
                $"Reconnaissance settings can only be set for schedules with type 'Reconnaissance'. Current type: '{schedule.ScheduleType}'.");
        }

        // Validate time constraints
        if (command.MinArrivalTime <= command.MinDepartureTime)
        {
            return Result.Failure<ReconnaissanceSettingsDto>(
                "MinArrivalTime must be after MinDepartureTime.");
        }

        if (command.MaxArrivalTime <= command.MinArrivalTime)
        {
            return Result.Failure<ReconnaissanceSettingsDto>(
                "MaxArrivalTime must be after MinArrivalTime.");
        }

        // Validate numeric constraints
        if (command.MinSpyCount < 1)
        {
            return Result.Failure<ReconnaissanceSettingsDto>(
                "MinSpyCount must be at least 1.");
        }

        if (command.MinDistanceToFront < 0)
        {
            return Result.Failure<ReconnaissanceSettingsDto>(
                "MinDistanceToFront cannot be negative.");
        }

        if (command.MaxPopulationInSourceVillage < 0)
        {
            return Result.Failure<ReconnaissanceSettingsDto>(
                "MaxPopulationInSourceVillage cannot be negative.");
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
