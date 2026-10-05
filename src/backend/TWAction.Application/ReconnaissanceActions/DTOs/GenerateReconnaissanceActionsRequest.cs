namespace TWAction.Application.ReconnaissanceActions.DTOs;

/// <summary>
/// Request DTO matching Generator.Api's GenerateReconnaissanceActionsRequest.
/// </summary>
public sealed record GenerateReconnaissanceActionsRequest
{
    public required DateTimeOffset MinDepartureTime { get; init; }
    public required DateTimeOffset MinArrivalTime { get; init; }
    public required DateTimeOffset MaxArrivalTime { get; init; }
    public required int MinDistanceToFront { get; init; }
    public required int MinSpyCount { get; init; }
    public required int MaxPopulationInSourceVillage { get; init; }
    public required bool SkipNightSendings { get; init; }
    public required IReadOnlyList<VillageDto> AllyVillages { get; init; }
    public required IReadOnlyList<VillageSmallDto> EnemyVillages { get; init; }
}
