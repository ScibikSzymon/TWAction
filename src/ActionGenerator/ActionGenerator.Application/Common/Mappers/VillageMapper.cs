using ActionGenerator.Application.Common.DTOs;
using ActionGenerator.Domain.Entities;

namespace ActionGenerator.Application.Common.Mappers;

internal static class VillageMapper
{
    public static VillageSmallDto ToSmallDto(this Village village)
    {
        return new VillageSmallDto
        {
            Id = village.Id,
            PlayerId = village.PlayerId,
            X = village.Coordinates.X,
            Y = village.Coordinates.Y
        };
    }

    public static IReadOnlyList<VillageSmallDto> ToSmallDtos(this IEnumerable<Village> villages)
    {
        return villages.Select(ToSmallDto).ToList();
    }
}
