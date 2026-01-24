using Riok.Mapperly.Abstractions;
using TWAction.Application.Schedules.DTOs;
using TWAction.Domain.Schedules;

namespace TWAction.Application.Mappers;

[Mapper]
public static partial class ITroopsStateMapper
{
    public static TroopsStateDto ToDto(TroopsStateEntity troopsState) => new TroopsStateDto
    {
        Id = troopsState.Id,
        ScheduleId = troopsState.ScheduleId,
        CreatedAt = troopsState.CreatedAt,
        UpdatedAt = troopsState.UpdatedAt
    };
}
