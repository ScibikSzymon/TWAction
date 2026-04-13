namespace TWAction.Application.MainActions.Commands;

/// <summary>Triggers main action generation for a schedule.</summary>
public sealed record GenerateMainActionsCommand(Guid ScheduleId);

/// <summary>Summary of the generation result returned to the caller.</summary>
public sealed record GenerateMainActionsResponse
{
    public required Guid ScheduleId { get; init; }
    public required int GeneratedCommandsCount { get; init; }
    public required int TargetGroupCount { get; init; }
    public required int TargetVillageCount { get; init; }
}
