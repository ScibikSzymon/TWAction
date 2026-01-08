namespace TWAction.Application.DTOs;

public sealed class UserDto
{
    public Guid Id { get; set; }

    public string Email { get; set; } = null!;

    public string? DisplayName { get; set; }

    public string Provider { get; set; } = null!;

    public DateTimeOffset CreatedAt { get; set; }
}
