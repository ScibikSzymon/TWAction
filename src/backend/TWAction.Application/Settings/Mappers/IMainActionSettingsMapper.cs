using Riok.Mapperly.Abstractions;
using TWAction.Application.Settings.DTOs;
using TWAction.Domain.Settings;

namespace TWAction.Application.Mappers;

[Mapper]
public static partial class IMainActionSettingsMapper
{
    public static MainActionSettingsDto ToDto(MainActionSettings settings) => new MainActionSettingsDto
    {
        Id = settings.Id,
        ScheduleId = settings.ScheduleId,
        MinDepartureTime = settings.MinDepartureTime,
        SkipNightSendings = settings.SkipNightSendings,
        MaxNobleDistance = settings.MaxNobleDistance,
        OffSettings = new MainActionOffSettingsDto
        {
            MinOffUnits = settings.OffSettings.MinOffUnits,
            MinDistanceFromFront = settings.OffSettings.MinDistanceFromFront
        },
        CatasSettings = new MainActionCatasSettingsDto
        {
            MinCatasNumber = settings.CatasSettings.MinCatasNumber,
            MinDistanceFromFront = settings.CatasSettings.MinDistanceFromFront,
            MaxOffUnits = settings.CatasSettings.MaxOffUnits
        },
        FakeOffSettings = new MainActionFakeOffSettingsDto
        {
            MinOffUnits = settings.FakeOffSettings.MinOffUnits,
            MinDistanceFromFront = settings.FakeOffSettings.MinDistanceFromFront
        },
        FakeDeffSettings = new MainActionFakeDeffSettingsDto
        {
            MaxOffUnits = settings.FakeDeffSettings.MaxOffUnits,
            MinDistanceFromFront = settings.FakeDeffSettings.MinDistanceFromFront
        },
        NobleSettings = new MainActionNobleSettingsDto
        {
            MinDistanceFromFront = settings.NobleSettings.MinDistanceFromFront,
            MinOffUnitsForOffNoble = settings.NobleSettings.MinOffUnitsForOffNoble,
            MinOffUnitsForFakeOffNoble = settings.NobleSettings.MinOffUnitsForFakeOffNoble,
            MaxOffUnitsForDefNoble = settings.NobleSettings.MaxOffUnitsForDefNoble,
            MinDeffUnitsForDefNoble = settings.NobleSettings.MinDeffUnitsForDefNoble
        },
        PlayerNobleBudgets = settings.PlayerNobleBudgets
    };
}
