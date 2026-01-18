namespace TWAction.Domain.Schedules;

public class ScheduleEntity
{
    public Guid Id { get; set; }
    public Guid UserGuid { get; set; }
    public string Name { get; set; } = null!;
    public DateTime CreationDate { get; set; }
    public WorldType World { get; set; }
    public ScheduleType ScheduleType { get; set; }
}
