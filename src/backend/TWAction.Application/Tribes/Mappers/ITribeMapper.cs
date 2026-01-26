using Riok.Mapperly.Abstractions;
using TWAction.Application.Tribes.DTOs;
using TWAction.Domain.Tribes;

namespace TWAction.Application.Mappers;

[Mapper]
public static partial class ITribeMapper
{
    public static TribeDto ToDto(TribeInfo tribe) => new TribeDto
    {
        TribalWarsId = tribe.TribalWarsId,
        Name = tribe.Name,
        Short = tribe.Short,
        VillagesCount = tribe.VillagesCount
    };

    public static IReadOnlyList<TribeDto> ToDtos(IReadOnlyList<TribeInfo> tribes) => 
        tribes.Select(ToDto).ToList();
}

