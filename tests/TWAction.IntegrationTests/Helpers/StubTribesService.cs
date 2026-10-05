using System;
using TWAction.Application.Tribes.Interfaces;
using TWAction.Domain.Schedules;
using TWAction.Domain.Tribes;

namespace TWAction.IntegrationTests.Helpers;

public sealed class StubTribesService : ITribesService
{
    public Task<IReadOnlyList<TribeInfo>> GetTribesAsync(WorldType world, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<TribeInfo>>(Array.Empty<TribeInfo>());
    }

    public Task<Dictionary<int, PlayerInfo>> GetPlayersAsync(WorldType world, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new Dictionary<int, PlayerInfo>());
    }

    public Task<(Dictionary<int, VillageInfo> ById, Dictionary<int, VillageInfo> ByCoordinates)> GetVillagesAsync(
        WorldType world, 
        CancellationToken cancellationToken = default)
    {
        var emptyById = new Dictionary<int, VillageInfo>();
        var emptyByCoords = new Dictionary<int, VillageInfo>();
        return Task.FromResult((emptyById, emptyByCoords));
    }
}
