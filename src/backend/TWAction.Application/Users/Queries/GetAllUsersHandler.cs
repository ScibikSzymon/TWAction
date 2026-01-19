using TWAction.Application.Common;
using TWAction.Application.Mappers;
using TWAction.Application.Users.DTOs;
using TWAction.Application.Users.Interfaces;

namespace TWAction.Application.Users.Queries;

public sealed class GetAllUsersQuery;

public class GetAllUsersHandler(IUserRepository userRepository)
{
    public async Task<Result<IEnumerable<UserDto>>> Handle(GetAllUsersQuery query, CancellationToken cancellationToken = default)
    {
        var users = await userRepository.ListAllAsync(cancellationToken);
        var userDtos = users.Select(IUserMapper.ToDto).ToList();
        return Result.Success<IEnumerable<UserDto>>(userDtos);
    }
}
