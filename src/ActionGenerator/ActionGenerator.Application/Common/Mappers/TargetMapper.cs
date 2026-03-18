using ActionGenerator.Application.Common.DTOs;
using ActionGenerator.Domain.Entities;
using ActionGenerator.Domain.Enums;

namespace ActionGenerator.Application.Common.Mappers;

internal static class TargetMapper
{
    public static Target ToEntity(
        this VillageSmallDto dto, 
        DateTimeOffset minArrivalTime, 
        DateTimeOffset maxArrivalTime)
    {
        return new Target
        {
            Id = dto.Id,
            PlayerId = dto.PlayerId,
            Coordinates = new Coordinates { X = dto.X, Y = dto.Y },
            MinArrivalTime = minArrivalTime,
            MaxArrivalTime = maxArrivalTime,
            CommandType = CommandType.Reconnaissance, // Always Reconnaissance for now
            CommnadNumber = 1
        };
    }

    public static IReadOnlyList<Target> ToEntities(
        this IReadOnlyList<VillageSmallDto> dtos, 
        DateTimeOffset minArrivalTime, 
        DateTimeOffset maxArrivalTime)
    {
        return dtos.Select(dto => dto.ToEntity(minArrivalTime, maxArrivalTime)).ToList();
    }
}

