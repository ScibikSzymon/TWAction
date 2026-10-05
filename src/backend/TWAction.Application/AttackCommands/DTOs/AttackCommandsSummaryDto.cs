namespace TWAction.Application.AttackCommands.DTOs;

/// <summary>
/// Summary of generated attack commands for a schedule.
/// </summary>
public sealed record AttackCommandsSummaryDto
{
    public required int TotalCount { get; init; }
    public required DateTimeOffset FirstMinDepartureTime { get; init; }
    public required DateTimeOffset LastMinDepartureTime { get; init; }
    public required IReadOnlyDictionary<string, int> CountByType { get; init; }
    public required DateTimeOffset GeneratedAt { get; init; }
}
