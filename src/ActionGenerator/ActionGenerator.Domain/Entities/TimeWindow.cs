namespace ActionGenerator.Domain.Entities;

public sealed record TimeWindow
{
    public DateTimeOffset MinDepartureTime { get; init; }
    public DateTimeOffset MaxDepartureTime { get; init; }
    public DateTimeOffset MinArrivalTime { get; init; }
    public DateTimeOffset MaxArrivalTime { get; init; }
}
