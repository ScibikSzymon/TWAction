using Microsoft.EntityFrameworkCore;
using TWAction.Application.Schedules.Interfaces;
using TWAction.Domain.Schedules;

namespace TWAction.Persistence.Repositories;

public sealed class TroopsUploadRepository(TWActionDbContext db) : ITroopsUploadRepository
{
    public async Task<IReadOnlyList<TroopsUploadEntity>> ListRecentAsync(Guid scheduleId, DateTimeOffset since, CancellationToken ct = default) =>
        await db.TroopsUploads.AsNoTracking().Where(x => x.ScheduleId == scheduleId && x.UploadedAt > since)
            .OrderBy(x => x.UploadedAt).ToListAsync(ct);

    public async Task RecordAsync(TroopsUploadEntity upload, CancellationToken ct = default)
    {
        db.TroopsUploads.Add(upload);
        await db.SaveChangesAsync(ct);
    }
}
