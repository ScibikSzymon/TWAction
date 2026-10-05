namespace TWAction.Application.Settings.DTOs;

public sealed record ReconnaissanceSettingsDto
{
    public required Guid Id { get; init; }
    
    public required Guid ScheduleId { get; init; }
    
    public required DateTimeOffset MinDepartureTime { get; init; }
    
    public required DateTimeOffset MinArrivalTime { get; init; }
    
    public required DateTimeOffset MaxArrivalTime { get; init; }
    
    public required int MinDistanceToFront { get; init; }
    
    public required int MinSpyCount { get; init; }
    
    public required int MaxPopulationInSourceVillage { get; init; }
    
    public required bool SkipNightSendings { get; init; }
}

public sealed record SaveReconnaissanceSettingsRequest
{
    public required DateTimeOffset MinDepartureTime { get; init; }
    
    public required DateTimeOffset MinArrivalTime { get; init; }
    
    public required DateTimeOffset MaxArrivalTime { get; init; }
    
    public required int MinDistanceToFront { get; init; }
    
    public required int MinSpyCount { get; init; }
    
    public required int MaxPopulationInSourceVillage { get; init; }
    
    public required bool SkipNightSendings { get; init; }
}
