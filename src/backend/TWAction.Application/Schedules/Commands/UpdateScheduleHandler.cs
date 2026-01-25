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
    string World,
    string ScheduleType,
    List<int> EnemyTribalWarsIds
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

        if (!Enum.TryParse<WorldType>(command.World, ignoreCase: true, out var world))
        {
            return Result.Failure<ScheduleDto>($"Invalid world value '{command.World}'.");
        }

        if (!Enum.TryParse<ScheduleType>(command.ScheduleType, ignoreCase: true, out var scheduleType))
        {
            return Result.Failure<ScheduleDto>($"Invalid schedule type value '{command.ScheduleType}'.");
        }

        // Clear enemies if world changed
        if (world != schedule.World)
        {
            schedule.Enemies.Clear();
        }

        schedule.World = world;
        schedule.ScheduleType = scheduleType;

        // Handle enemies if provided        
        if (command.EnemyTribalWarsIds != null)
        {
            if (command.EnemyTribalWarsIds.Any())
            {
                try
                {
                    var tribes = await tribesService.GetTribesAsync(world, cancellationToken);

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

