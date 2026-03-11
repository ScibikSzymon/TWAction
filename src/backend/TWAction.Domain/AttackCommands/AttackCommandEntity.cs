namespace TWAction.Domain.AttackCommands;

/// <summary>
/// Represents an attack command generated for a schedule.
/// </summary>
public sealed class AttackCommandEntity
{
    public Guid Id { get; set; }
    public Guid ScheduleId { get; set; }

    // Time window
    public DateTimeOffset MinDepartureTime { get; set; }
    public DateTimeOffset MaxDepartureTime { get; set; }
    public DateTimeOffset MinArrivalTime { get; set; }
    public DateTimeOffset MaxArrivalTime { get; set; }

    // Source village
    public int SourceVillageId { get; set; }
    public int SourceX { get; set; }
    public int SourceY { get; set; }
    public int SourcePlayerId { get; set; }

    // Destination village
    public int DestinationVillageId { get; set; }
    public int DestinationX { get; set; }
    public int DestinationY { get; set; }
    public int DestinationPlayerId { get; set; }

    public string CommandType { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
