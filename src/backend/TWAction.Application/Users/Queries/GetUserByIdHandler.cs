using TWAction.Application.Common;
using TWAction.Application.Mappers;
using TWAction.Application.Users.DTOs;
using TWAction.Application.Users.Interfaces;

namespace TWAction.Application.Users.Queries;

public sealed record GetUserByIdQuery(Guid UserId);

public sealed class GetUserByIdHandler(IUserRepository userRepository)
{
    public async Task<Result<UserDto>> Handle(GetUserByIdQuery query, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(query.UserId, cancellationToken);

        return user is null
            ? Result.Failure<UserDto>($"User with ID '{query.UserId}' not found.")
            : Result.Success(IUserMapper.ToDto(user));
    }
}
