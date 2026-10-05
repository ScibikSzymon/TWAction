namespace ActionGenerator.Application.Common.DTOs;

public sealed record VillageSmallDto
{
    public required int X { get; init; }
    public required int Y { get; init; }
    public required int Id { get; init; }
    public required int PlayerId { get; init; }
}
