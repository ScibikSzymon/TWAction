using System;
using TWAction.Application.Tribes.Interfaces;
using TWAction.Domain.Schedules;
using TWAction.Domain.Tribes;

namespace TWAction.IntegrationTests.Helpers;

public sealed class StubTribesService : ITribesService
{
    /// <summary>
    /// Returns an empty tribes list for any world.
    /// </summary>
    public Task<IReadOnlyList<TribeInfo>> GetTribesAsync(WorldType world, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<TribeInfo>>(Array.Empty<TribeInfo>());
    }

    /// <summary>
    /// Returns an empty players dictionary for any world.
    /// </summary>
    public Task<Dictionary<int, PlayerInfo>> GetPlayersAsync(WorldType world, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new Dictionary<int, PlayerInfo>());
    }

    /// <summary>
    /// Returns empty village dictionaries for any world.
    /// </summary>
    public Task<(Dictionary<int, VillageInfo> ById, Dictionary<int, VillageInfo> ByCoordinates)> GetVillagesAsync(
        WorldType world, 
        CancellationToken cancellationToken = default)
    {
        var emptyById = new Dictionary<int, VillageInfo>();
        var emptyByCoords = new Dictionary<int, VillageInfo>();
        return Task.FromResult((emptyById, emptyByCoords));
    }
}
