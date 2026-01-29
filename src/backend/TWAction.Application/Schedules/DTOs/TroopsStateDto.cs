namespace TWAction.Application.Schedules.DTOs;

public sealed record TroopsStateDto
{
    public required Guid Id { get; init; }

    public required Guid ScheduleId { get; init; }

    public required int VillageCount { get; init; }

    public required int PlayerCount { get; init; }

    public required DateTimeOffset CreatedAt { get; init; }

    public required DateTimeOffset UpdatedAt { get; init; }
}

public sealed record UploadTroopsStateRequest
{
    public required string RawData { get; init; }
}

