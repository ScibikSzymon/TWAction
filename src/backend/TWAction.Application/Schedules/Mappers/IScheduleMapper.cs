using Riok.Mapperly.Abstractions;
using TWAction.Application.Schedules.DTOs;
using TWAction.Domain.Schedules;

namespace TWAction.Application.Mappers;

[Mapper]
public static partial class IScheduleMapper
{
    [MapProperty(nameof(ScheduleEntity.UserGuid), nameof(ScheduleDto.UserId))]
    public static partial ScheduleDto ToDto(ScheduleEntity schedule);
}
