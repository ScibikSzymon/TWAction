using Microsoft.EntityFrameworkCore;
using TWAction.Application.Schedules.Interfaces;
using TWAction.Domain.Schedules;

namespace TWAction.Persistence.Repositories;

public class TroopsStateRepository(TWActionDbContext db) : ITroopsStateRepository
{
    public async Task<TroopsStateEntity?> GetByScheduleIdAsync(Guid scheduleId, CancellationToken cancellationToken = default)
    {
        return await db.TroopsStates.FirstOrDefaultAsync(t => t.ScheduleId == scheduleId, cancellationToken);
    }

    public async Task<TroopsStateEntity> CreateAsync(TroopsStateEntity troopsState, CancellationToken cancellationToken = default)
    {
        await db.TroopsStates.AddAsync(troopsState, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return troopsState;
    }

    public async Task<TroopsStateEntity> UpdateAsync(TroopsStateEntity troopsState, CancellationToken cancellationToken = default)
    {
        db.TroopsStates.Update(troopsState);
        await db.SaveChangesAsync(cancellationToken);
        return troopsState;
    }

    public async Task DeleteByScheduleIdAsync(Guid scheduleId, CancellationToken cancellationToken = default)
    {
        var troopsState = await db.TroopsStates.FirstOrDefaultAsync(t => t.ScheduleId == scheduleId, cancellationToken);
        if (troopsState is not null)
        {
            db.TroopsStates.Remove(troopsState);
            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
