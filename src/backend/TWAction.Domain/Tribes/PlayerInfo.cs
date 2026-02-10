namespace TWAction.Domain.Tribes;

/// <summary>
/// Represents a player from TribalWars map data.
/// </summary>
public sealed class PlayerInfo
{
    /// <summary>
    /// Player ID in TribalWars system.
    /// </summary>
    public required int Id { get; init; }

    /// <summary>
    /// Player nickname (URL decoded).
    /// </summary>
    public required string Nick { get; init; }

    /// <summary>
    /// ID of the tribe this player belongs to (0 if no tribe).
    /// </summary>
    public required int TribeId { get; init; }

    /// <summary>
    /// Player rank in the world.
    /// </summary>
    public int Rank { get; init; }

    /// <summary>
    /// Total points of the player.
    /// </summary>
    public int Points { get; init; }

    /// <summary>
    /// Number of villages owned by the player.
    /// </summary>
    public int VillagesCount { get; init; }
}
