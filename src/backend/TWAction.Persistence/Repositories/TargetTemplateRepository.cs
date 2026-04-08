using Microsoft.EntityFrameworkCore;
using TWAction.Application.Templates.Interfaces;
using TWAction.Domain.Templates;

namespace TWAction.Persistence.Repositories;

/// <summary>EF Core implementation of <see cref="ITargetTemplateRepository"/>.</summary>
public sealed class TargetTemplateRepository(TWActionDbContext context) : ITargetTemplateRepository
{
    public async Task<IEnumerable<TargetTemplate>> GetAllAsync(Guid userId, CancellationToken ct = default) =>
        await context.TargetTemplates
            .Where(t => t.IsDefault || t.UserId == userId)
            .ToListAsync(ct);

    public async Task<TargetTemplate?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await context.TargetTemplates.FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task<TargetTemplate> CreateAsync(TargetTemplate template, CancellationToken ct = default)
    {
        await context.TargetTemplates.AddAsync(template, ct);
        await context.SaveChangesAsync(ct);
        return template;
    }

    public async Task UpdateAsync(TargetTemplate template, CancellationToken ct = default)
    {
        context.TargetTemplates.Update(template);
        await context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(TargetTemplate template, CancellationToken ct = default)
    {
        context.TargetTemplates.Remove(template);
        await context.SaveChangesAsync(ct);
    }

    public async Task<bool> DefaultTemplatesExistAsync(CancellationToken ct = default) =>
        await context.TargetTemplates.AnyAsync(t => t.IsDefault, ct);
}
