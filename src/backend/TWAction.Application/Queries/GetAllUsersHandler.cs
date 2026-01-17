using TWAction.Application.Common;
using TWAction.Application.DTOs;
using TWAction.Application.Interfaces;
using TWAction.Application.Mappers;

namespace TWAction.Application.Handlers;

public sealed class GetAllUsersQuery { }

public class GetAllUsersHandler
{
    private readonly IUserRepository _userRepository;

    public GetAllUsersHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<IEnumerable<UserDto>>> Handle(GetAllUsersQuery query, CancellationToken cancellationToken = default)
    {
        var users = await _userRepository.ListAllAsync(cancellationToken);
        var userDtos = users.Select(IUserMapper.ToDto).ToList();
        return Result.Success<IEnumerable<UserDto>>(userDtos);
    }
}
