using Microsoft.EntityFrameworkCore;
using TWAction.Application.TargetGroups.Interfaces;
using TWAction.Domain.TargetGroups;

namespace TWAction.Persistence.Repositories;

public sealed class TargetGroupRepository(TWActionDbContext context) : ITargetGroupRepository
{
    public async Task<IEnumerable<TargetGroup>> GetAllAsync(Guid scheduleId, CancellationToken ct = default) =>
        await context.TargetGroups.Where(g => g.ScheduleId == scheduleId).ToListAsync(ct);

    public async Task<TargetGroup?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await context.TargetGroups.FirstOrDefaultAsync(g => g.Id == id, ct);

    public async Task<TargetGroup> CreateAsync(TargetGroup group, CancellationToken ct = default)
    {
        await context.TargetGroups.AddAsync(group, ct);
        await context.SaveChangesAsync(ct);
        return group;
    }

    public async Task UpdateAsync(TargetGroup group, CancellationToken ct = default)
    {
        context.TargetGroups.Update(group);
        await context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(TargetGroup group, CancellationToken ct = default)
    {
        context.TargetGroups.Remove(group);
        await context.SaveChangesAsync(ct);
    }
}
