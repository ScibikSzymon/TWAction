using Riok.Mapperly.Abstractions;
using TWAction.Application.Settings.DTOs;
using TWAction.Domain.Settings;

namespace TWAction.Application.Mappers;

[Mapper]
public static partial class IReconnaissanceSettingsMapper
{
    public static ReconnaissanceSettingsDto ToDto(ReconnaissanceSettings settings) => new ReconnaissanceSettingsDto
    {
        Id = settings.Id,
        ScheduleId = settings.ScheduleId,
        MinDepartureTime = settings.MinDepartureTime,
        MinArrivalTime = settings.MinArrivalTime,
        MaxArrivalTime = settings.MaxArrivalTime,
        MinDistanceToFront = settings.MinDistanceToFront,
        MinSpyCount = settings.MinSpyCount,
        MaxPopulationInSourceVillage = settings.MaxPopulationInSourceVillage,
        SkipNightSendings = settings.SkipNightSendings
    };
}
