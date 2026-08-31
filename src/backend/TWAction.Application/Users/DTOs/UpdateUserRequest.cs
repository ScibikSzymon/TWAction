using TWAction.Domain.Users;

namespace TWAction.Application.Users.DTOs;

public sealed record UpdateUserRequest
{
    public required string Email { get; init; }

    public string? DisplayName { get; init; }

    public required UserRole Role { get; init; }
}
