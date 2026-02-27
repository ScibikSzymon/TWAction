namespace TWAction.Application.Schedules.DTOs;

using TWAction.Domain.Schedules;

public sealed record ScheduleDto
{
    public required Guid Id { get; init; }

    public required Guid UserId { get; init; }

    public required string Name { get; init; }

    public required DateTimeOffset CreationDate { get; init; }

    public required WorldType World { get; init; }

    public required ScheduleType ScheduleType { get; init; }

    public required IReadOnlyList<int> EnemyIds { get; init; }

    public DateTimeOffset? SentToPlemionaRozpiskiAt { get; init; }
}




