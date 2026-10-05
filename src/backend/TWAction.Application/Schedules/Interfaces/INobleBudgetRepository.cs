using TWAction.Domain.Schedules;

namespace TWAction.Application.Schedules.Interfaces;

public interface INobleBudgetRepository
{
    Task<IReadOnlyList<NobleBudgetEntity>> GetByScheduleIdAsync(Guid scheduleId, CancellationToken cancellationToken = default);

    Task<NobleBudgetEntity?> GetByScheduleAndPlayerIdAsync(Guid scheduleId, int playerId, CancellationToken cancellationToken = default);

    Task<NobleBudgetEntity> CreateAsync(NobleBudgetEntity nobleBudget, CancellationToken cancellationToken = default);

    Task<NobleBudgetEntity> UpdateAsync(NobleBudgetEntity nobleBudget, CancellationToken cancellationToken = default);

    Task UpsertManyAsync(IEnumerable<NobleBudgetEntity> nobleBudgets, CancellationToken cancellationToken = default);

    Task DeleteByScheduleIdAsync(Guid scheduleId, CancellationToken cancellationToken = default);
}
