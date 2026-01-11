namespace TWAction.Domain.Entities;

public sealed class UserSessionEntity
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public DateTimeOffset ExpiresAt { get; set; } //TODO: how to set TTL in DB so record is expired automatically?

    public string? Data { get; set; }
}
