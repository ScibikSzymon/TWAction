namespace ActionGenerator.Application.Common.DTOs;

public sealed record ArmyDto
{
    public  required uint Spear { get; init; }
    public  required uint Sword { get; init; }
    public  required uint Axe { get; init; }
    public  required uint Archer { get; init; }
    public  required uint Spy { get; init; }
    public  required uint Light { get; init; }
    public  required uint HorseArcher { get; init; }
    public  required uint Heavy { get; init; }
    public  required uint Ram { get; init; }
    public  required uint Catapult { get; init; }
    public  required uint Noble { get; init; }
}
