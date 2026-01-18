using Riok.Mapperly.Abstractions;
using TWAction.Application.Users.DTOs;
using TWAction.Domain.Users;

namespace TWAction.Application.Mappers;

[Mapper]
public static partial class IUserMapper
{
    public static partial UserDto ToDto(UserEntity user);
}
