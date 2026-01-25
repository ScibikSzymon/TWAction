namespace TWAction.Domain.Tribes;

public sealed class TribeInfo
{
    public required int TribalWarsId { get; init; }

    public required string Name { get; init; }

    public required string Short { get; init; }

    public required int VillagesCount { get; init; }
}
