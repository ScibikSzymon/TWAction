namespace ActionGenerator.Domain.Entities;

public class SourceVillage : Village
{
    public Army Army { get; init; } = null!;
}
