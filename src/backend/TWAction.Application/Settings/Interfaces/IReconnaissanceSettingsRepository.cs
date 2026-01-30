using TWAction.Domain.Settings;

namespace TWAction.Application.Settings.Interfaces;

public interface IReconnaissanceSettingsRepository
{
    Task<ReconnaissanceSettings?> GetByScheduleIdAsync(Guid scheduleId, CancellationToken cancellationToken = default);
    
    Task<ReconnaissanceSettings> CreateAsync(ReconnaissanceSettings settings, CancellationToken cancellationToken = default);
    
    Task<ReconnaissanceSettings> UpdateAsync(ReconnaissanceSettings settings, CancellationToken cancellationToken = default);
}
