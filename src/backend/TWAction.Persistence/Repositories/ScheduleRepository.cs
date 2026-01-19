using Microsoft.EntityFrameworkCore;
using TWAction.Application.Schedules.Interfaces;
using TWAction.Domain.Schedules;

namespace TWAction.Persistence.Repositories;

public class ScheduleRepository : IScheduleRepository
{
    private readonly TWActionDbContext _db;

    public ScheduleRepository(TWActionDbContext db) => _db = db;

    public async Task<ScheduleEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.Schedules.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<ScheduleEntity>> ListByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _db.Schedules
            .Where(s => s.UserGuid == userId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<ScheduleEntity> AddAsync(ScheduleEntity schedule, CancellationToken cancellationToken = default)
    {
        await _db.Schedules.AddAsync(schedule, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
        return schedule;
    }

    public async Task<ScheduleEntity> UpdateAsync(ScheduleEntity schedule, CancellationToken cancellationToken = default)
    {
        _db.Schedules.Update(schedule);
        await _db.SaveChangesAsync(cancellationToken);
        return schedule;
    }

    public async Task DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var schedule = await _db.Schedules.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        if (schedule is not null)
        {
            _db.Schedules.Remove(schedule);
            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}
