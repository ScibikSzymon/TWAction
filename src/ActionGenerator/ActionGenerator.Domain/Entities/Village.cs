using ActionGenerator.Domain.Common.ValueObjects;

namespace ActionGenerator.Domain.Entities;

public class Village
{
    public int Id { get; init; }
    public int PlayerId { get; init; }
    public Coordinates Coordinates { get; init; }= null!;
    public int DistanceToFront { get; set; }
}
