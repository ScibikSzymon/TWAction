namespace TWAction.Domain.Schedules;

public sealed class NobleBudgetEntity
{
    public Guid Id { get; set; }

    public Guid ScheduleId { get; set; }

    /// <summary>
    /// Player ID from TribalWars API
    /// </summary>
    public int PlayerId { get; set; }

    /// <summary>
    /// Maximum number of nobles this player can use
    /// </summary>
    public int Budget { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
