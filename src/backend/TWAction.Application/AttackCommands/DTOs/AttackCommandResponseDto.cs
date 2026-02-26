using TWAction.Application.ReconnaissanceActions.DTOs;

namespace TWAction.Application.AttackCommands.DTOs;

/// <summary>
/// DTO for returning an attack command to the client.
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
