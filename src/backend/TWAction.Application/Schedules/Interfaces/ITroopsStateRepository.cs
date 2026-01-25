using TWAction.Domain.Schedules;

namespace TWAction.Application.Schedules.Interfaces;

public interface ITroopsStateRepository
{
    Task<TroopsStateEntity?> GetByScheduleIdAsync(Guid scheduleId, CancellationToken cancellationToken = default);

    Task<TroopsStateEntity> CreateAsync(TroopsStateEntity troopsState, CancellationToken cancellationToken = default);

    Task<TroopsStateEntity> UpdateAsync(TroopsStateEntity troopsState, CancellationToken cancellationToken = default);

    Task DeleteByScheduleIdAsync(Guid scheduleId, CancellationToken cancellationToken = default);
}
