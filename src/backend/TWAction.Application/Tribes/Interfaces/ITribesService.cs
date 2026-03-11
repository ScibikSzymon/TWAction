using TWAction.Domain.Schedules;
using TWAction.Domain.Tribes;

namespace TWAction.Application.Tribes.Interfaces;

public interface ITribesService
{
    /// <summary>
    /// Fetches tribes from TribalWars API with caching.
    /// </summary>
    Task<IReadOnlyList<TribeInfo>> GetTribesAsync(WorldType world, CancellationToken cancellationToken = default);

    /// <summary>
    /// Fetches players from TribalWars API with caching.
    /// </summary>
    /// <returns>Dictionary mapping player ID to PlayerInfo</returns>
    Task<Dictionary<int, PlayerInfo>> GetPlayersAsync(WorldType world, CancellationToken cancellationToken = default);

    /// <summary>
    /// Fetches villages from TribalWars API with caching and links them with player data.
    /// </summary>
    /// <returns>Tuple with ID-based and coordinate-based village dictionaries</returns>
    Task<(Dictionary<int, VillageInfo> ById, Dictionary<int, VillageInfo> ByCoordinates)> GetVillagesAsync(
        WorldType world, 
        CancellationToken cancellationToken = default);
}



