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
    /// <param name="world">The world to query.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An empty list of tribes.</returns>
    public Task<IReadOnlyList<TribeInfo>> GetTribesAsync(WorldType world, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<TribeInfo>>(Array.Empty<TribeInfo>());
    }
}
