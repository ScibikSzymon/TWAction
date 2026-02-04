namespace ActionGenerator.Domain.Entities;

public sealed record TimeWindowDto
{
    public DateTimeOffset MinDepartureTime { get; init; }
    public DateTimeOffset MaxDepartureTime { get; init; }
    public DateTimeOffset MinArrivalTime { get; init; }
    public DateTimeOffset MaxArrivalTime { get; init; }
}
