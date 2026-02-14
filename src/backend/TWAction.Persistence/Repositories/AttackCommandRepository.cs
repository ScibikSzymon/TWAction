using Microsoft.EntityFrameworkCore;
using TWAction.Application.ReconnaissanceActions.Interfaces;
using TWAction.Domain.ReconnaissanceActions;

namespace TWAction.Persistence.Repositories;

public sealed class AttackCommandRepository(TWActionDbContext context) : IAttackCommandRepository
{
    public async Task SaveCommandsAsync(
        Guid scheduleId,
        IReadOnlyList<AttackCommandEntity> commands,
        CancellationToken cancellationToken = default)
    {
        // Delete existing commands for this schedule (replace strategy)
        await DeleteByScheduleIdAsync(scheduleId, cancellationToken);

        // Add new commands
        if (commands.Count > 0)
        {
            await context.AttackCommands.AddRangeAsync(commands, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<IReadOnlyList<AttackCommandEntity>> GetByScheduleIdAsync(
        Guid scheduleId,
        CancellationToken cancellationToken = default)
    {
        return await context.AttackCommands
            .Where(c => c.ScheduleId == scheduleId)
            .OrderBy(c => c.MinDepartureTime)
            .ToListAsync(cancellationToken);
    }

    public async Task DeleteByScheduleIdAsync(Guid scheduleId, CancellationToken cancellationToken = default)
    {
        await context.AttackCommands
            .Where(c => c.ScheduleId == scheduleId)
            .ExecuteDeleteAsync(cancellationToken);
    }
}
