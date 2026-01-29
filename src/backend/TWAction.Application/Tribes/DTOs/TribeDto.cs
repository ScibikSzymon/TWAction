namespace TWAction.Application.Tribes.DTOs;

public sealed record TribeDto
{
    public required int TribalWarsId { get; init; }

    public required string Name { get; init; }

    public required string Short { get; init; }

    public required int VillagesCount { get; init; }
}
