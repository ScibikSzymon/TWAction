namespace TWAction.Domain.Schedules;

using TWAction.Domain.Tribes;

public class ScheduleEntity
{
    public Guid Id { get; set; }
    
    public Guid UserGuid { get; set; }
    
    public string Name { get; set; } = null!;
    
    public DateTimeOffset CreationDate { get; set; }
    
    public WorldType World { get; set; }
    
    public ScheduleType ScheduleType { get; set; }
    
    public List<TribeInfo> Enemies { get; set; } = new();
}


