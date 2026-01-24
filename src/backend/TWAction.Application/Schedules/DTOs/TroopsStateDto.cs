namespace TWAction.Application.Schedules.DTOs;

public sealed record TroopsStateDto
{
    public required Guid Id { get; init; }

    public required Guid ScheduleId { get; init; }

    public required DateTime CreatedAt { get; init; }

    public required DateTime UpdatedAt { get; init; }
}

public sealed record UploadTroopsStateRequest
{
    public required string RawData { get; init; }
}
