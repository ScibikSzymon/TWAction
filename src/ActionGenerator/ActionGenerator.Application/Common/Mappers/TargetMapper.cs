using ActionGenerator.Application.Common.DTOs;
using ActionGenerator.Domain.Common.ValueObjects;
using ActionGenerator.Domain.Entities;
using ActionGenerator.Domain.Enums;

namespace ActionGenerator.Application.Common.Mappers;

public static class TargetMapper
{
    public static Target ToEntity(
        VillageSmallDto dto, 
        DateTimeOffset minArrivalTime, 
        DateTimeOffset maxArrivalTime,
        CommandType commandType)
    {
        return new Target
        {
            Id = dto.Id,
            PlayerId = dto.PlayerId,
            Coordinates = new Coordinates { X = dto.X, Y = dto.Y },
            MinArrivalTime = minArrivalTime,
            MaxArrivalTime = maxArrivalTime,
            CommandType = CommandType.Reconnaissance
        };
    }

    public static IReadOnlyList<Target> ToEntities(
        IReadOnlyList<VillageSmallDto> dtos, 
        DateTimeOffset minArrivalTime, 
        DateTimeOffset maxArrivalTime,
        CommandType commandType)
    {
        return dtos.Select(dto => ToEntity(dto, minArrivalTime, maxArrivalTime, commandType)).ToList();
    }
}
