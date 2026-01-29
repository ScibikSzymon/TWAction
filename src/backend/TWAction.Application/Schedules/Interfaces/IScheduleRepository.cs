using TWAction.Domain.Schedules;

namespace TWAction.Application.Schedules.Interfaces;

public interface IScheduleRepository
{
    Task<ScheduleEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ScheduleEntity>> ListByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<ScheduleEntity> AddAsync(ScheduleEntity schedule, CancellationToken cancellationToken = default);

    Task<ScheduleEntity> UpdateAsync(ScheduleEntity schedule, CancellationToken cancellationToken = default);

    Task DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
