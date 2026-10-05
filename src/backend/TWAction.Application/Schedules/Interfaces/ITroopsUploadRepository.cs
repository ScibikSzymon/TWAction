using TWAction.Domain.Schedules;

namespace TWAction.Application.Schedules.Interfaces;

public interface ITroopsUploadRepository
{
    Task<IReadOnlyList<TroopsUploadEntity>> ListRecentAsync(Guid scheduleId, DateTimeOffset since, CancellationToken ct = default);
    Task RecordAsync(TroopsUploadEntity upload, CancellationToken ct = default);
}
