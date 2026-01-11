using TWAction.Application.DTOs;
using TWAction.Application.Interfaces;
using TWAction.Application.Mappers;

namespace TWAction.Application.Handlers;

public sealed record GetUserBySessionQuery(Guid SessionId);

public class GetUserBySessionHandler
{
    private readonly IUserSessionRepository _sessionRepository;
    private readonly IUserRepository _userRepository;

    public GetUserBySessionHandler(IUserSessionRepository sessionRepository, IUserRepository userRepository)
    {
        _sessionRepository = sessionRepository;
        _userRepository = userRepository;
    }

    public async Task<UserDto?> Handle(GetUserBySessionQuery query, CancellationToken cancellationToken = default)
    {
        var session = await _sessionRepository.GetByIdAsync(query.SessionId, cancellationToken);
        if (session is null) return null;
        if (session.ExpiresAt < DateTimeOffset.UtcNow) return null;

        var user = await _userRepository.GetByIdAsync(session.UserId, cancellationToken);
        if (user is null) return null;

        return IUserMapper.ToDto(user);
    }
}
