namespace ActionGenerator.Domain.Actions;

/// <summary>
/// Represents a generated reconnaissance action
/// </summary>
public sealed class ReconnaissanceAction
{
    public required string SourceVillage { get; init; }
    
    public required string TargetVillage { get; init; }
    
    public required int SpyCount { get; init; }
    
    public required DateTimeOffset DepartureTime { get; init; }
    
    public required DateTimeOffset ArrivalTime { get; init; }
    
    public required int Distance { get; init; }
}
