using TWAction.Application.Common;
using TWAction.Application.Mappers;
using TWAction.Application.Schedules.DTOs;
using TWAction.Application.Schedules.Interfaces;
using TWAction.Application.Tribes.Interfaces;

namespace TWAction.Application.Schedules.Commands;

public sealed record SetScheduleEnemiesCommand(Guid ScheduleId, List<int> EnemyTribalWarsIds);

public class SetScheduleEnemiesHandler(
    IScheduleRepository scheduleRepository,
    ITribesService tribesService)
{
    public async Task<Result<ScheduleDto>> Handle(SetScheduleEnemiesCommand command, CancellationToken cancellationToken = default)
    {
        // Get schedule
        var schedule = await scheduleRepository.GetByIdAsync(command.ScheduleId, cancellationToken);
        if (schedule is null)
        {
            return Result.Failure<ScheduleDto>($"Schedule with ID '{command.ScheduleId}' not found.");
        }

        // Get all tribes for the schedule's world
        var tribesResult = await tribesService.GetTribesAsync(schedule.World, cancellationToken);
        if (tribesResult.IsFailure)
        {
            return Result.Failure<ScheduleDto>($"Failed to fetch tribes: {tribesResult.Error}");
        }

        // Filter tribes by enemy IDs
        var enemies = tribesResult.Value
            .Where(t => command.EnemyTribalWarsIds.Contains(t.TribalWarsId))
            .ToList();

        // Validate that all provided IDs exist
        if (enemies.Count != command.EnemyTribalWarsIds.Count)
        {
            var notFound = command.EnemyTribalWarsIds
                .Except(enemies.Select(e => e.TribalWarsId))
                .ToList();
            return Result.Failure<ScheduleDto>($"The following tribe IDs were not found: {string.Join(", ", notFound)}");
        }

        // Update schedule with enemies
        schedule.Enemies = enemies;
        await scheduleRepository.UpdateAsync(schedule, cancellationToken);

        return Result.Success(IScheduleMapper.ToDto(schedule));
    }
}
