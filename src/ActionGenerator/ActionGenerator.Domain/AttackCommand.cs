namespace ActionGenerator.Domain.Entities;

public sealed class AttackCommand
{
    public Village Source { get; init; } = null!;
    public Target Target { get; init; } = null!;
    public DateTimeOffset MinimalDepartureTime { get; init; }
    public DateTimeOffset MaximalDepartureTime { get; init; }
}