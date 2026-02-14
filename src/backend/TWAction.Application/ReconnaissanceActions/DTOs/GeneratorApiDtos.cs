namespace TWAction.Application.ReconnaissanceActions.DTOs;

/// <summary>
/// DTO matching Generator.Api's AttackCommandDto response.
/// </summary>
public sealed record AttackCommandDto
{
    public required TimeWindowDto TimeWindow { get; init; }
    public required VillageSmallDto Source { get; init; }
    public required VillageSmallDto Destination { get; init; }
    public required string CommandType { get; init; }
}

public sealed record TimeWindowDto
{
    public DateTimeOffset MinDepartureTime { get; init; }
    public DateTimeOffset MaxDepartureTime { get; init; }
    public DateTimeOffset MinArrivalTime { get; init; }
    public DateTimeOffset MaxArrivalTime { get; init; }
}

public sealed record VillageSmallDto
{
    public required int X { get; init; }
    public required int Y { get; init; }
    public required int Id { get; init; }
    public required int PlayerId { get; init; }
}

public sealed record VillageDto
{
    public required int Id { get; init; }
    public required int PlayerId { get; init; }
    public required int X { get; init; }
    public required int Y { get; init; }
    public required ArmyDto Army { get; init; }
}

public sealed record ArmyDto
{
    public required uint Spear { get; init; }
    public required uint Sword { get; init; }
    public required uint Axe { get; init; }
    public required uint Archer { get; init; }
    public required uint Spy { get; init; }
    public required uint Light { get; init; }
    public required uint HorseArcher { get; init; }
    public required uint Heavy { get; init; }
    public required uint Ram { get; init; }
    public required uint Catapult { get; init; }
    public required uint Noble { get; init; }
}
