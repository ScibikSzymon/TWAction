namespace ActionGenerator.Application.Common.DTOs;

public sealed record ArmyDto
{
    public int Spear { get; init; }
    public int Sword { get; init; }
    public int Axe { get; init; }
    public int Archer { get; init; }
    public int Spy { get; init; }
    public int Light { get; init; }
    public int HorseArcher { get; init; }
    public int Heavy { get; init; }
    public int Ram { get; init; }
    public int Catapult { get; init; }
    public int Noble { get; init; }
}
