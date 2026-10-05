namespace TWAction.Application.ReconnaissanceActions.DTOs;

public sealed record GenerateReconnaissanceActionsResponse
{
    public required Guid ScheduleId { get; init; }
    public required int GeneratedCommandsCount { get; init; }
}
