using ActionGenerator.Application.Common.DTOs;

public sealed record GenerateMainActionRequest
{
    public required DateTimeOffset MinDepartureTime { get; init; }
    public required bool SkipNightSendings { get; init; }
    public required IReadOnlyList<VillageDto> AllyVillages { get; init; }
    public required IReadOnlyList<TargetDto> Targets { get; init; }
}
