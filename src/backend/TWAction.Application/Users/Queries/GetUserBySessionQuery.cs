using TWAction.Application.Common;
using TWAction.Application.Mappers;
using TWAction.Application.Users.DTOs;
using TWAction.Application.Users.Interfaces;

namespace TWAction.Application.Users.Queries;

public sealed record GetUserBySessionQuery(Guid SessionId);

public class GetUserBySessionHandler(IUserSessionRepository sessionRepository, IUserRepository userRepository)
{
    public async Task<Result<UserDto>> Handle(GetUserBySessionQuery query, CancellationToken cancellationToken = default)
    {
        var session = await sessionRepository.GetByIdAsync(query.SessionId, cancellationToken);
        if (session is null)
        {
            return Result.Failure<UserDto>($"Session with ID '{query.SessionId}' not found.");
        }

        if (session.ExpiresAt < DateTimeOffset.UtcNow)
        {
            return Result.Failure<UserDto>("Session has expired.");
        }

        var user = await userRepository.GetByIdAsync(session.UserId, cancellationToken);
        if (user is null)
        {
            return Result.Failure<UserDto>($"User with ID '{session.UserId}' not found.");
        }

        return Result.Success(IUserMapper.ToDto(user));
    }
}
