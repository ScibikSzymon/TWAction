using TWAction.Application.Common;
using TWAction.Domain.Schedules;
using TWAction.Domain.Tribes;

namespace TWAction.Application.Tribes.Interfaces;

public interface ITribesService
{
    Task<Result<List<TribeInfo>>> GetTribesAsync(WorldType world, CancellationToken cancellationToken = default);
}

