namespace TWAction.Domain.Schedules;

public sealed class TroopsStateEntity
{
    public Guid Id { get; set; }

    public Guid ScheduleId { get; set; }

    /// <summary>
    /// Compressed troops data stored as base64 encoded gzip
    /// </summary>
    public required string CompressedData { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}

