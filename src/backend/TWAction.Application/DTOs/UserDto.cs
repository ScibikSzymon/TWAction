namespace TWAction.Application.DTOs;

public sealed record UserDto
{
    public required Guid Id { get; init; }

    public required string Email { get; init; }

    public string? DisplayName { get; init; }

    public required string Provider { get; init; }

    public required DateTimeOffset CreatedAt { get; init; }
}
