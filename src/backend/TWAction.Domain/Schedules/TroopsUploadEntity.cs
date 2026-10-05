namespace TWAction.Domain.Schedules;

public sealed class TroopsUploadEntity
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid ScheduleId { get; set; }
    public DateTimeOffset UploadedAt { get; set; }
}
