namespace ActionGenerator.Application.Common.DTOs;

public sealed record TargetDto
{
    public required DateTimeOffset MinArrivalTime { get; init; }
    public required DateTimeOffset MaxArrivalTime { get; init; }
    public required CommandType CommandType { get; init; }
    public required uint CommandNumber { get; init; }
    public required VillageSmallDto Village { get; init; }
}
