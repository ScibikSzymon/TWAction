namespace TWAction.Application.AttackCommands.Queries;

/// <summary>
/// Query to get all attack commands for a schedule.
/// </summary>
public sealed record GetAttackCommandsQuery(Guid ScheduleId);
