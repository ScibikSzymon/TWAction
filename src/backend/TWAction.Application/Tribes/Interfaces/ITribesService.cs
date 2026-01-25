using TWAction.Domain.Schedules;
using TWAction.Domain.Tribes;

namespace TWAction.Application.Tribes.Interfaces;

public interface ITribesService
{
    Task<IReadOnlyList<TribeInfo>> GetTribesAsync(WorldType world, CancellationToken cancellationToken = default);
}



