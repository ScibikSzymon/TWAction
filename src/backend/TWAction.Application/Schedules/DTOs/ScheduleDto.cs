namespace TWAction.Application.Schedules.DTOs;

using TWAction.Domain.Schedules;

public sealed record ScheduleDto
{
    public required Guid Id { get; init; }

    public required Guid UserId { get; init; }

    public required string Name { get; init; }

    public required DateTime CreationDate { get; init; }

    public required WorldType World { get; init; }

    public required ScheduleType ScheduleType { get; init; }

    public required List<int> EnemyIds { get; init; }
}


