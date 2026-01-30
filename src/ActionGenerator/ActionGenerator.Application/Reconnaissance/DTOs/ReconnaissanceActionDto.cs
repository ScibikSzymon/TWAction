namespace ActionGenerator.Application.Reconnaissance.DTOs;

public sealed record ReconnaissanceActionDto
{
    public required string SourceVillage { get; init; }
    
    public required string TargetVillage { get; init; }
    
    public required int SpyCount { get; init; }
    
    public required DateTimeOffset DepartureTime { get; init; }
    
    public required DateTimeOffset ArrivalTime { get; init; }
    
    public required int Distance { get; init; }
}

public sealed record GenerateReconnaissanceActionsRequest
{
    public required DateTimeOffset MinDepartureTime { get; init; }
    
    public required DateTimeOffset MinArrivalTime { get; init; }
    
    public required DateTimeOffset MaxArrivalTime { get; init; }
    
    public required int MinDistanceToFront { get; init; }
    
    public required int MinSpyCount { get; init; }
    
    public required int MaxPopulationInSourceVillage { get; init; }
    
    public required bool SkipNightSendings { get; init; }
    
    public required IReadOnlyList<string> SourceVillages { get; init; }
    
    public required IReadOnlyList<string> TargetVillages { get; init; }
}
