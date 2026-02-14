namespace TWAction.Application.ReconnaissanceActions.Commands;

/// <summary>
/// Command to generate reconnaissance actions for a schedule.
/// </summary>
public sealed record GenerateReconnaissanceActionsCommand(Guid ScheduleId);
