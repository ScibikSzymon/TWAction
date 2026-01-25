namespace TWAction.Application.Schedules.DTOs;

public sealed record SetScheduleEnemiesRequest
{
    public required List<int> EnemyTribalWarsIds { get; init; }
}
