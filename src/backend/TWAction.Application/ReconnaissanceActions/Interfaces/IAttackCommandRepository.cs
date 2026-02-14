using TWAction.Domain.ReconnaissanceActions;

namespace TWAction.Application.ReconnaissanceActions.Interfaces;

public interface IAttackCommandRepository
{
    /// <summary>
    /// Saves a batch of attack commands for a schedule.
    /// Deletes existing commands for the schedule before saving new ones.
    /// </summary>
    Task SaveCommandsAsync(Guid scheduleId, IReadOnlyList<AttackCommandEntity> commands, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all attack commands for a specific schedule.
    /// </summary>
    Task<IReadOnlyList<AttackCommandEntity>> GetByScheduleIdAsync(Guid scheduleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes all attack commands for a specific schedule.
    /// </summary>
    Task DeleteByScheduleIdAsync(Guid scheduleId, CancellationToken cancellationToken = default);
}
