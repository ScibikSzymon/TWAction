using TWAction.Application.Common;
using TWAction.Application.Mappers;
using TWAction.Application.Schedules.DTOs;
using TWAction.Application.Schedules.Interfaces;
using TWAction.Application.Tribes.Interfaces;
using TWAction.Domain.Schedules;
using TWAction.Domain.Tribes;

namespace TWAction.Application.Schedules.Commands;

public sealed record UpdateScheduleCommand(
    Guid ScheduleId,
    string Name,
    WorldType World,
    ScheduleType ScheduleType,
    IReadOnlyList<int>? EnemyTribalWarsIds = null
);


public class UpdateScheduleHandler(
    IScheduleRepository scheduleRepository,
    ITribesService tribesService)
{
    public async Task<Result<ScheduleDto>> Handle(UpdateScheduleCommand command, CancellationToken cancellationToken = default)
    {
        var schedule = await scheduleRepository.GetByIdAsync(command.ScheduleId, cancellationToken);

        if (schedule is null)
        {
            return Result.Failure<ScheduleDto>($"Schedule with ID '{command.ScheduleId}' not found.");
        }

        if (string.IsNullOrWhiteSpace(command.Name))
        {
            return Result.Failure<ScheduleDto>("Schedule name cannot be empty.");
        }

        schedule.Name = command.Name;

        // Clear enemies if world changed
        if (command.World != schedule.World)
        {
            schedule.Enemies.Clear();
        }

        schedule.World = command.World;
        schedule.ScheduleType = command.ScheduleType;

        // Handle enemies if provided        
        if (command.EnemyTribalWarsIds != null)
        {
            if (command.EnemyTribalWarsIds.Any())
            {
                try
                {
                    var tribes = await tribesService.GetTribesAsync(command.World, cancellationToken);

                    var enemies = tribes
                        .Where(t => command.EnemyTribalWarsIds.Contains(t.TribalWarsId))
                        .ToList();

                    if (enemies.Count != command.EnemyTribalWarsIds.Count)
                    {
                        var notFound = command.EnemyTribalWarsIds
                            .Except(enemies.Select(e => e.TribalWarsId))
                            .ToList();
                        return Result.Failure<ScheduleDto>($"The following tribe IDs were not found: {string.Join(", ", notFound)}");
                    }

                    schedule.Enemies = enemies;
                }
                catch (Exception ex)
                {
                    return Result.Failure<ScheduleDto>($"Failed to fetch tribes: {ex.Message}");
                }
            }
            else
            {
                // Empty list means clear enemies
                schedule.Enemies = new List<TribeInfo>();
            }
        }

        await scheduleRepository.UpdateAsync(schedule, cancellationToken);

        return Result.Success(IScheduleMapper.ToDto(schedule));

    }
}

