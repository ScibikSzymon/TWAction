using TWAction.Domain.Settings;

namespace TWAction.Application.Settings.Interfaces;

public interface IMainActionSettingsRepository
{
    Task<MainActionSettings?> GetByScheduleIdAsync(Guid scheduleId, CancellationToken cancellationToken = default);
    
    Task<MainActionSettings> CreateAsync(MainActionSettings settings, CancellationToken cancellationToken = default);
    
    Task<MainActionSettings> UpdateAsync(MainActionSettings settings, CancellationToken cancellationToken = default);
}
