namespace TWAction.Domain.Entities;

public sealed class UserEntity
{
    public Guid Id { get; set; }

    public string Email { get; set; } = null!;

    public string? DisplayName { get; set; }

    public string Provider { get; set; } = "google";

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
