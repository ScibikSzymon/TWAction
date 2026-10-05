namespace TWAction.Domain.Settings;

public sealed class ReconnaissanceSettings
{
    public Guid Id { get; set; }
    public Guid ScheduleId { get; set; }
    public DateTimeOffset MinDepartureTime { get; set; }
    public DateTimeOffset MinArrivalTime { get; set; }
    public DateTimeOffset MaxArrivalTime { get; set; }
    public int MinDistanceToFront { get; set; }
    public int MinSpyCount { get; set; }
    public int MaxPopulationInSourceVillage { get; set; }
    public bool SkipNightSendings { get; set; }
}
