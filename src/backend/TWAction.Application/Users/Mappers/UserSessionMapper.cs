using TWAction.Application.Users.DTOs;
using TWAction.Domain.Users;

namespace TWAction.Application.Mappers;

public static class UserSessionMapper
{
    public static UserSessionDto ToDto(UserSessionEntity session)
    {
        return new UserSessionDto
        {
            Id = session.Id,
            UserId = session.UserId,
            ExpiresAt = session.ExpiresAt,
            IsActive = session.ExpiresAt >= DateTimeOffset.UtcNow
        };
    }
}
