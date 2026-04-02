using TWAction.Application.Common;
using TWAction.Application.Mappers;
using TWAction.Application.Schedules.Interfaces;
using TWAction.Application.Settings.DTOs;
using TWAction.Application.Settings.Interfaces;
using TWAction.Domain.Schedules;
using TWAction.Domain.Settings;

namespace TWAction.Application.Settings.Commands;

public sealed record SaveMainActionSettingsCommand(
    Guid ScheduleId,
    DateTimeOffset MinDepartureTime,
    bool SkipNightSendings,
    uint MaxNobleDistance,
    MainActionOffSettingsDto OffSettings,
    MainActionCatasSettingsDto CatasSettings,
    MainActionFakeOffSettingsDto FakeOffSettings,
    MainActionFakeDeffSettingsDto FakeDeffSettings,
    MainActionNobleSettingsDto NobleSettings,
    IReadOnlyDictionary<int, uint> PlayerNobleBudgets
);

public class SaveMainActionSettingsHandler(
    IMainActionSettingsRepository settingsRepository,
    IScheduleRepository scheduleRepository)
{
    public async Task<Result<MainActionSettingsDto>> Handle(
        SaveMainActionSettingsCommand command,
        CancellationToken cancellationToken = default)
    {
        // Validate schedule exists
        var schedule = await scheduleRepository.GetByIdAsync(command.ScheduleId, cancellationToken);
        if (schedule is null)
        {
            return Result.Failure<MainActionSettingsDto>(
                $"Schedule with ID '{command.ScheduleId}' not found.");
        }

        // Validate schedule type is Main
        if (schedule.ScheduleType != ScheduleType.Main)
        {
            return Result.Failure<MainActionSettingsDto>(
                $"Main action settings can only be set for schedules with type 'Main'. Current type: '{schedule.ScheduleType}'.");
        }

        // Validate MaxNobleDistance
        if (command.MaxNobleDistance is < 10 or > 99)
        {
            return Result.Failure<MainActionSettingsDto>(
                "MaxNobleDistance must be between 10 and 99.");
        }

        // Validate OffSettings
        if (command.OffSettings.MinOffUnits is < 1_000 or > 21_000)
        {
            return Result.Failure<MainActionSettingsDto>(
                "OffSettings.MinOffUnits must be between 1,000 and 21,000.");
        }

        if (command.OffSettings.MinDistanceFromFront > 100)
        {
            return Result.Failure<MainActionSettingsDto>(
                "OffSettings.MinDistanceFromFront cannot be greater than 100.");
        }

        // Validate CatasSettings
        if (command.CatasSettings.MinCatasNumber is < 10 or > 2_500)
        {
            return Result.Failure<MainActionSettingsDto>(
                "CatasSettings.MinCatasNumber must be between 10 and 2,500.");
        }

        if (command.CatasSettings.MinDistanceFromFront > 100)
        {
            return Result.Failure<MainActionSettingsDto>(
                "CatasSettings.MinDistanceFromFront cannot be greater than 100.");
        }

        if (command.CatasSettings.MaxOffUnits > 25_000)
        {
            return Result.Failure<MainActionSettingsDto>(
                "CatasSettings.MaxOffUnits cannot be greater than 25,000.");
        }

        // Validate FakeOffSettings
        if (command.FakeOffSettings.MinOffUnits is < 1_000 or > 21_000)
        {
            return Result.Failure<MainActionSettingsDto>(
                "FakeOffSettings.MinOffUnits must be between 1,000 and 21,000.");
        }

        if (command.FakeOffSettings.MinDistanceFromFront > 100)
        {
            return Result.Failure<MainActionSettingsDto>(
                "FakeOffSettings.MinDistanceFromFront cannot be greater than 100.");
        }

        // Validate FakeDeffSettings
        if (command.FakeDeffSettings.MaxOffUnits > 21_000)
        {
            return Result.Failure<MainActionSettingsDto>(
                "FakeDeffSettings.MaxOffUnits cannot be greater than 21,000.");
        }

        if (command.FakeDeffSettings.MinDistanceFromFront > 100)
        {
            return Result.Failure<MainActionSettingsDto>(
                "FakeDeffSettings.MinDistanceFromFront cannot be greater than 100.");
        }

        // Validate NobleSettings
        if (command.NobleSettings.MinDistanceFromFront > 30)
        {
            return Result.Failure<MainActionSettingsDto>(
                "NobleSettings.MinDistanceFromFront cannot be greater than 30.");
        }

        if (command.NobleSettings.MinOffUnitsForOffNoble is < 1_000 or > 21_000)
        {
            return Result.Failure<MainActionSettingsDto>(
                "NobleSettings.MinOffUnitsForOffNoble must be between 1,000 and 21,000.");
        }

        if (command.NobleSettings.MinOffUnitsForFakeOffNoble is < 1_000 or > 21_000)
        {
            return Result.Failure<MainActionSettingsDto>(
                "NobleSettings.MinOffUnitsForFakeOffNoble must be between 1,000 and 21,000.");
        }

        if (command.NobleSettings.MaxOffUnitsForDefNoble is < 1_000 or > 21_000)
        {
            return Result.Failure<MainActionSettingsDto>(
                "NobleSettings.MaxOffUnitsForDefNoble must be between 1,000 and 21,000.");
        }

        if (command.NobleSettings.MinDeffUnitsForDefNoble is < 1_000 or > 21_000)
        {
            return Result.Failure<MainActionSettingsDto>(
                "NobleSettings.MinDeffUnitsForDefNoble must be between 1,000 and 21,000.");
        }

        // Check if settings already exist (upsert)
        var existing = await settingsRepository.GetByScheduleIdAsync(command.ScheduleId, cancellationToken);

        MainActionSettings settings;

        if (existing is not null)
        {
            // Update existing
            existing.MinDepartureTime = command.MinDepartureTime;
            existing.SkipNightSendings = command.SkipNightSendings;
            existing.MaxNobleDistance = command.MaxNobleDistance;
            
            existing.OffSettings.MinOffUnits = command.OffSettings.MinOffUnits;
            existing.OffSettings.MinDistanceFromFront = command.OffSettings.MinDistanceFromFront;
            
            existing.CatasSettings.MinCatasNumber = command.CatasSettings.MinCatasNumber;
            existing.CatasSettings.MinDistanceFromFront = command.CatasSettings.MinDistanceFromFront;
            existing.CatasSettings.MaxOffUnits = command.CatasSettings.MaxOffUnits;
            
            existing.FakeOffSettings.MinOffUnits = command.FakeOffSettings.MinOffUnits;
            existing.FakeOffSettings.MinDistanceFromFront = command.FakeOffSettings.MinDistanceFromFront;
            
            existing.FakeDeffSettings.MaxOffUnits = command.FakeDeffSettings.MaxOffUnits;
            existing.FakeDeffSettings.MinDistanceFromFront = command.FakeDeffSettings.MinDistanceFromFront;
            
            existing.NobleSettings.MinDistanceFromFront = command.NobleSettings.MinDistanceFromFront;
            existing.NobleSettings.MinOffUnitsForOffNoble = command.NobleSettings.MinOffUnitsForOffNoble;
            existing.NobleSettings.MinOffUnitsForFakeOffNoble = command.NobleSettings.MinOffUnitsForFakeOffNoble;
            existing.NobleSettings.MaxOffUnitsForDefNoble = command.NobleSettings.MaxOffUnitsForDefNoble;
            existing.NobleSettings.MinDeffUnitsForDefNoble = command.NobleSettings.MinDeffUnitsForDefNoble;
            
            existing.PlayerNobleBudgets = new Dictionary<int, uint>(command.PlayerNobleBudgets);

            settings = await settingsRepository.UpdateAsync(existing, cancellationToken);
        }
        else
        {
            // Create new
            settings = new MainActionSettings
            {
                Id = Guid.NewGuid(),
                ScheduleId = command.ScheduleId,
                MinDepartureTime = command.MinDepartureTime,
                SkipNightSendings = command.SkipNightSendings,
                MaxNobleDistance = command.MaxNobleDistance,
                OffSettings = new MainActionOffSettings
                {
                    MinOffUnits = command.OffSettings.MinOffUnits,
                    MinDistanceFromFront = command.OffSettings.MinDistanceFromFront
                },
                CatasSettings = new MainActionCatasSettings
                {
                    MinCatasNumber = command.CatasSettings.MinCatasNumber,
                    MinDistanceFromFront = command.CatasSettings.MinDistanceFromFront,
                    MaxOffUnits = command.CatasSettings.MaxOffUnits
                },
                FakeOffSettings = new MainActionFakeOffSettings
                {
                    MinOffUnits = command.FakeOffSettings.MinOffUnits,
                    MinDistanceFromFront = command.FakeOffSettings.MinDistanceFromFront
                },
                FakeDeffSettings = new MainActionFakeDeffSettings
                {
                    MaxOffUnits = command.FakeDeffSettings.MaxOffUnits,
                    MinDistanceFromFront = command.FakeDeffSettings.MinDistanceFromFront
                },
                NobleSettings = new MainActionNobleSettings
                {
                    MinDistanceFromFront = command.NobleSettings.MinDistanceFromFront,
                    MinOffUnitsForOffNoble = command.NobleSettings.MinOffUnitsForOffNoble,
                    MinOffUnitsForFakeOffNoble = command.NobleSettings.MinOffUnitsForFakeOffNoble,
                    MaxOffUnitsForDefNoble = command.NobleSettings.MaxOffUnitsForDefNoble,
                    MinDeffUnitsForDefNoble = command.NobleSettings.MinDeffUnitsForDefNoble
                },
                PlayerNobleBudgets = new Dictionary<int, uint>(command.PlayerNobleBudgets)
            };

            settings = await settingsRepository.CreateAsync(settings, cancellationToken);
        }

        return Result.Success(IMainActionSettingsMapper.ToDto(settings));
    }
}
