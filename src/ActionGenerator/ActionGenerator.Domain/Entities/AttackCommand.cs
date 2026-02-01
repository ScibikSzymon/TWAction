using ActionGenerator.Domain.Common.ValueObjects;
using ActionGenerator.Domain.Enums;

namespace ActionGenerator.Domain.Entities;

public sealed class AttackCommand
{
    public Village Source { get; init; } = null!;
    public Village Destination { get; init; } = null!;
    public TimeWindow TimeWindow { get; init; } = null!;
    public CommandType CommandType { get; init; }
}
