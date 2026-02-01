using ActionGenerator.Domain.Common.ValueObjects;

namespace ActionGenerator.Application.Features.ReconnaissanceActions.DTOs;

public sealed record AttackCommandDto
{
    public required TimeWindow TimeWindow { get; init; }
    public required VillageSmallDto Source { get; init; }
    public required VillageSmallDto Destination { get; init; }
}
