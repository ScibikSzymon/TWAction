using TWAction.Application.ReconnaissanceActions.DTOs;

namespace TWAction.Application.ReconnaissanceActions.Commands;

/// <summary>
/// Query to get all attack commands for a schedule.
/// </summary>
public sealed record GetAttackCommandsQuery(Guid ScheduleId);
