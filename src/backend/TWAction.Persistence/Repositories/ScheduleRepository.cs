using Microsoft.EntityFrameworkCore;
using TWAction.Application.Schedules.Interfaces;
using TWAction.Domain.Schedules;

namespace TWAction.Persistence.Repositories;

public class ScheduleRepository(TWActionDbContext db) : IScheduleRepository
{
    public async Task<ScheduleEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await db.Schedules.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<ScheduleEntity>> ListByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await db.Schedules
            .Where(s => s.UserGuid == userId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<ScheduleEntity> AddAsync(ScheduleEntity schedule, CancellationToken cancellationToken = default)
    {
        await db.Schedules.AddAsync(schedule, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return schedule;
    }

    public async Task<ScheduleEntity> UpdateAsync(ScheduleEntity schedule, CancellationToken cancellationToken = default)
    {
        db.Schedules.Update(schedule);
        await db.SaveChangesAsync(cancellationToken);
        return schedule;
    }

    public async Task DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var schedule = await db.Schedules.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        if (schedule is not null)
        {
            db.Schedules.Remove(schedule);
            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
