namespace TWAction.Application.AttackCommands.Queries;

/// <summary>
/// Query to get a summary of generated attack commands for a schedule.
/// </summary>
public sealed record GetAttackCommandsSummaryQuery(Guid ScheduleId);
