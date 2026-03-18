using ActionGenerator.Domain.Enums;

namespace ActionGenerator.Domain.Entities;

public class Target : Village
{
    public required DateTimeOffset MinArrivalTime { get; init; }
    public required DateTimeOffset MaxArrivalTime { get; init; }
    public required CommandType CommandType { get; init; }
    public required uint CommnadNumber { get; init; }
}
