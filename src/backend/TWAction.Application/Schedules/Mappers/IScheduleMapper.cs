using Riok.Mapperly.Abstractions;
using TWAction.Application.Schedules.DTOs;
using TWAction.Domain.Schedules;

namespace TWAction.Application.Mappers;

[Mapper]
public static partial class IScheduleMapper
{
    [MapProperty(nameof(ScheduleEntity.UserGuid), nameof(ScheduleDto.UserId))]
    [MapProperty(nameof(ScheduleEntity.Enemies), nameof(ScheduleDto.Enemies))]
    public static ScheduleDto ToDto(ScheduleEntity schedule) => new ScheduleDto
    {
        Id = schedule.Id,
        UserId = schedule.UserGuid,
        Name = schedule.Name,
        CreationDate = schedule.CreationDate,
        World = schedule.World,
        ScheduleType = schedule.ScheduleType,
        Enemies = ITribeMapper.ToDtos(schedule.Enemies)
    };
}

