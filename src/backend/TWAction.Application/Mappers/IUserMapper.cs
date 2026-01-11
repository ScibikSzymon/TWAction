using Riok.Mapperly.Abstractions;
using TWAction.Application.DTOs;
using TWAction.Domain.Entities;

namespace TWAction.Application.Mappers;

[Mapper]
public static partial class IUserMapper
{
    public static partial UserDto ToDto(UserEntity user);
}
