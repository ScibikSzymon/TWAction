using TWAction.Domain.TargetGroups;

namespace TWAction.Application.TargetGroups.Interfaces;

/// <summary>Persistence contract for target group CRUD operations.</summary>
public interface ITargetGroupRepository
{
    Task<IEnumerable<TargetGroup>> GetAllAsync(Guid scheduleId, CancellationToken ct = default);
    Task<TargetGroup?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<TargetGroup> CreateAsync(TargetGroup group, CancellationToken ct = default);
    Task UpdateAsync(TargetGroup group, CancellationToken ct = default);
    Task DeleteAsync(TargetGroup group, CancellationToken ct = default);
}
