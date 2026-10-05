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
            CommandType = CommandType.Reconnaissance,
            CommandNumber = 1
        };
    }

    public static IReadOnlyList<Target> ToEntities(
        this IReadOnlyList<VillageSmallDto> dtos,
        DateTimeOffset minArrivalTime,
        DateTimeOffset maxArrivalTime)
    {
        return dtos.Select(dto => dto.ToEntity(minArrivalTime, maxArrivalTime)).ToList();
    }

    public static Target ToEntity(this TargetDto dto)
    {
        return new Target
        {
            Id = dto.Village.Id,
            PlayerId = dto.Village.PlayerId,
            Coordinates = new Coordinates { X = dto.Village.X, Y = dto.Village.Y },
            MinArrivalTime = dto.MinArrivalTime,
            MaxArrivalTime = dto.MaxArrivalTime,
            CommandType = dto.CommandType,
            CommandNumber = dto.CommandNumber
        };
    }

    public static IReadOnlyList<Target> ToEntities(this IReadOnlyList<TargetDto> dtos)
        => dtos.Select(ToEntity).ToList();
}

