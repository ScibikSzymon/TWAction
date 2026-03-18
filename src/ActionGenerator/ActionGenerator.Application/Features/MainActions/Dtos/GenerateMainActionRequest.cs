using ActionGenerator.Application.Common.DTOs;

public sealed record GenerateReconnaissanceActionsRequest
{
    public required DateTimeOffset MinDepartureTime { get; init; }
    public required bool SkipNightSendings { get; init; }
    public required IReadOnlyList<VillageDto> AllyVillages { get; init; }
    public required IReadOnlyList<TargetDto> EnemyVillages { get; init; }
}
