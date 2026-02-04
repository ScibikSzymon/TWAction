using ActionGenerator.Application.Common.DTOs;
using ActionGenerator.Domain.Entities;

namespace ActionGenerator.Application.Common.Mappers;

internal static class SourceVillageMapper
{
    public static SourceVillage ToEntity(this VillageDto dto)
    {
        return new SourceVillage
        {
            Id = dto.Id,
            PlayerId = dto.PlayerId,
            Coordinates = new Coordinates { X = dto.X, Y = dto.Y },
            Army = new Army
            {
                Spy = dto.Army.Spy,
                Spear = dto.Army.Spear,
                Sword = dto.Army.Sword,
                Axe = dto.Army.Axe,
                Archer = dto.Army.Archer,
                Light = dto.Army.Light,
                HorseArcher = dto.Army.HorseArcher,
                Heavy = dto.Army.Heavy,
                Ram = dto.Army.Ram,
                Catapult = dto.Army.Catapult,
                Noble = dto.Army.Noble
            }
        };
    }

    public static IReadOnlyList<SourceVillage> ToEntities(this IReadOnlyList<VillageDto> dtos)
    {
        return dtos.Select(ToEntity).ToList();
    }
}
