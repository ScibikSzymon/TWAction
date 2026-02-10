namespace TWAction.Domain.Tribes;

/// <summary>
/// Represents a village from TribalWars map data.
/// </summary>
public sealed class VillageInfo
{
    /// <summary>
    /// Village ID in TribalWars system.
    /// </summary>
    public required int Id { get; init; }

    /// <summary>
    /// Village name (URL decoded).
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// X coordinate on the map.
    /// </summary>
    public required int X { get; init; }

    /// <summary>
    /// Y coordinate on the map.
    /// </summary>
    public required int Y { get; init; }

    /// <summary>
    /// Village points (strength).
    /// </summary>
    public required int Points { get; init; }

    /// <summary>
    /// ID of the player who owns this village.
    /// </summary>
    public required int PlayerId { get; init; }

    /// <summary>
    /// Player information (optional, populated when needed).
    /// </summary>
    public PlayerInfo? Player { get; set; }

    /// <summary>
    /// Combined coordinate key for quick lookups (X * 1000 + Y).
    /// </summary>
    public int CoordinateKey => X * 1000 + Y;
}
