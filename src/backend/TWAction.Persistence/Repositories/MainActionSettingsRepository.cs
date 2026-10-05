using Microsoft.EntityFrameworkCore;
using TWAction.Application.Settings.Interfaces;
using TWAction.Domain.Settings;

namespace TWAction.Persistence.Repositories;

public sealed class MainActionSettingsRepository(TWActionDbContext context) : IMainActionSettingsRepository
{
    public async Task<MainActionSettings?> GetByScheduleIdAsync(Guid scheduleId, CancellationToken cancellationToken = default)
    {
        return await context.Set<MainActionSettings>()
            .FirstOrDefaultAsync(x => x.ScheduleId == scheduleId, cancellationToken);
    }

    public async Task<MainActionSettings> CreateAsync(MainActionSettings settings, CancellationToken cancellationToken = default)
    {
        await context.Set<MainActionSettings>().AddAsync(settings, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return settings;
    }

    public async Task<MainActionSettings> UpdateAsync(MainActionSettings settings, CancellationToken cancellationToken = default)
    {
        context.Set<MainActionSettings>().Update(settings);
        await context.SaveChangesAsync(cancellationToken);
        return settings;
    }
}
