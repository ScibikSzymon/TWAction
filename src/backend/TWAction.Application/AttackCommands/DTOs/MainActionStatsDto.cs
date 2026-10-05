namespace TWAction.Application.AttackCommands.DTOs;

/// <summary>
/// Statistics for a single player's involvement in the action (either as source or target).
/// </summary>
public sealed record CommandsPerPlayerDto
{
    public required int PlayerId { get; init; }
    public required string PlayerName { get; init; }
    public required int TotalCount { get; init; }
    public required IReadOnlyDictionary<string, int> CountByType { get; init; }
}

/// <summary>
/// Distribution of commands in a single hour (based on min arrival time).
/// </summary>
public sealed record CommandsPerHourDto
{
    public required int Hour { get; init; }
    public required int TotalCount { get; init; }
    public required IReadOnlyDictionary<string, int> CountByType { get; init; }
}

/// <summary>
/// Distribution of departure (send) commands within an 8-hour slot of a specific date.
/// SlotStart is always 0, 8, or 16.
/// </summary>
public sealed record CommandsPerDeparturePeriodDto
{
    /// <summary>Date portion of the period (UTC).</summary>
    public required DateOnly Date { get; init; }

    /// <summary>Start hour of the 8-hour slot: 0, 8, or 16.</summary>
    public required int SlotStart { get; init; }

    public required int TotalCount { get; init; }
    public required IReadOnlyDictionary<string, int> CountByType { get; init; }
}

/// <summary>
/// Full statistics for a generated main action, including breakdowns by enemy player,
/// source player, arrival hour and departure 8-hour periods.
/// </summary>
public sealed record MainActionStatsDto
{
    public required int TotalCommands { get; init; }
    public required IReadOnlyList<CommandsPerPlayerDto> CommandsPerEnemyPlayer { get; init; }
    public required IReadOnlyList<CommandsPerPlayerDto> CommandsPerSourcePlayer { get; init; }
    public required IReadOnlyList<CommandsPerHourDto> CommandsPerArrivalHour { get; init; }
    public required IReadOnlyList<CommandsPerDeparturePeriodDto> CommandsPerDeparturePeriod { get; init; }
}
