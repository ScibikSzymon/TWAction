namespace ActionGenerator.Application.Common.DTOs;

public sealed record ArmyDto
{
    public  required int Spear { get; init; }
    public  required int Sword { get; init; }
    public  required int Axe { get; init; }
    public  required int Archer { get; init; }
    public  required int Spy { get; init; }
    public  required int Light { get; init; }
    public  required int HorseArcher { get; init; }
    public  required int Heavy { get; init; }
    public  required int Ram { get; init; }
    public  required int Catapult { get; init; }
    public  required int Noble { get; init; }
}
