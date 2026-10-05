using TWAction.Application.Common;
using TWAction.Application.Mappers;
using TWAction.Application.Users.DTOs;
using TWAction.Application.Users.Interfaces;

namespace TWAction.Application.Users.Queries;

public sealed record GetUserSessionsQuery(Guid UserId);

public sealed class GetUserSessionsHandler(
    IUserRepository userRepository,
    IUserSessionRepository sessionRepository)
{
    public async Task<Result<IEnumerable<UserSessionDto>>> Handle(
        GetUserSessionsQuery query,
        CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(query.UserId, cancellationToken);

        if (user is null)
        {
            return Result.Failure<IEnumerable<UserSessionDto>>($"User with ID '{query.UserId}' not found.");
        }

        var sessions = await sessionRepository.ListByUserIdAsync(query.UserId, cancellationToken);
        return Result.Success<IEnumerable<UserSessionDto>>(sessions.Select(UserSessionMapper.ToDto).ToList());
    }
}
