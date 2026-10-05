namespace TWAction.Application.Users.DTOs;

public sealed record UserSessionDto
{
    public required Guid Id { get; init; }

    public required Guid UserId { get; init; }

    public required DateTimeOffset ExpiresAt { get; init; }

    public required bool IsActive { get; init; }
}
