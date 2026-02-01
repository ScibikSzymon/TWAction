using ActionGenerator.Domain.Entities;

namespace ActionGenerator.Application.Common.DTOs;

public sealed record AttackCommandDto
{
    public required TimeWindowDto TimeWindow { get; init; }
    public required VillageSmallDto Source { get; init; }
    public required VillageSmallDto Destination { get; init; }
}
