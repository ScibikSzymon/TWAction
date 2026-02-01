namespace ActionGenerator.Application.Features.ReconnaissanceActions.DTOs;

public sealed record VillageDto
{
    public required int Id { get; init; }
    public required int PlayerId { get; init; }
    public required int X { get; init; }
    public required int Y { get; init; }
    public required ArmyDto Army { get; init; }
}
