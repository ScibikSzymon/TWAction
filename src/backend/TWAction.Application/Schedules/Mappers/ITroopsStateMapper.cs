using Riok.Mapperly.Abstractions;
using TWAction.Application.Schedules.DTOs;
using TWAction.Domain.Schedules;

namespace TWAction.Application.Mappers;

[Mapper]
public static partial class ITroopsStateMapper
{
    /// <summary>
    /// Maps TroopsStateEntity to DTO without CompressedData
    /// VillageCount and PlayerCount must be set separately
    /// </summary>
    public static TroopsStateDto ToDto(TroopsStateEntity troopsState) => new TroopsStateDto
    {
        Id = troopsState.Id,
        ScheduleId = troopsState.ScheduleId,
        VillageCount = 0, // Must be set by handler
        PlayerCount = 0, // Must be set by handler
        CreatedAt = troopsState.CreatedAt,
        UpdatedAt = troopsState.UpdatedAt
    };
}
