namespace TWAction.Application.ReconnaissanceActions.DTOs;

/// <summary>
/// DTO for returning attack command to the client.
/// </summary>
public sealed record AttackCommandResponseDto
{
    public required Guid Id { get; init; }
    public required TimeWindowDto TimeWindow { get; init; }
    public required VillageSmallDto Source { get; init; }
    public required VillageSmallDto Destination { get; init; }
    public required string CommandType { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
}
