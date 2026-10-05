using Microsoft.EntityFrameworkCore;
using TWAction.Application.Settings.Interfaces;
using TWAction.Domain.Settings;

namespace TWAction.Persistence.Repositories;

public sealed class ReconnaissanceSettingsRepository(TWActionDbContext context) : IReconnaissanceSettingsRepository
{
    public async Task<ReconnaissanceSettings?> GetByScheduleIdAsync(Guid scheduleId, CancellationToken cancellationToken = default)
    {
        return await context.Set<ReconnaissanceSettings>()
            .FirstOrDefaultAsync(x => x.ScheduleId == scheduleId, cancellationToken);
    }

    public async Task<ReconnaissanceSettings> CreateAsync(ReconnaissanceSettings settings, CancellationToken cancellationToken = default)
    {
        await context.Set<ReconnaissanceSettings>().AddAsync(settings, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return settings;
    }

    public async Task<ReconnaissanceSettings> UpdateAsync(ReconnaissanceSettings settings, CancellationToken cancellationToken = default)
    {
        context.Set<ReconnaissanceSettings>().Update(settings);
        await context.SaveChangesAsync(cancellationToken);
        return settings;
    }
}
