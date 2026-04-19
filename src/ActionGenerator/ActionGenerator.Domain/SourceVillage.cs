namespace ActionGenerator.Domain.Entities;

public class SourceVillage : Village
{
    public Army Army { get; init; } = null!;
    public DateTimeOffset? OffComeBackTime { get; init; }
    public DateTimeOffset? CatasComeBackTime { get; init; }
}
