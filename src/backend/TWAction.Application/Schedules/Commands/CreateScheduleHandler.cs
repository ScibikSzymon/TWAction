using TWAction.Application.Common;
using TWAction.Application.Mappers;
using TWAction.Application.Schedules.DTOs;
using TWAction.Application.Schedules.Interfaces;
using TWAction.Application.Tribes.Interfaces;
using TWAction.Application.Users.Interfaces;
using TWAction.Domain.Schedules;
using TWAction.Domain.Tribes;

namespace TWAction.Application.Schedules.Commands;

public sealed record CreateScheduleCommand(
    Guid UserId,
    string Name,
    string World,
    string ScheduleType,
    IReadOnlyList<int> EnemyTribalWarsIds
);


public class CreateScheduleHandler(
    IScheduleRepository scheduleRepository,
    IUserRepository userRepository,
    ITribesService tribesService)
{
    public async Task<Result<ScheduleDto>> Handle(CreateScheduleCommand command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.Name))
        {
            return Result.Failure<ScheduleDto>("Schedule name cannot be empty.");
        }

        var user = await userRepository.GetByIdAsync(command.UserId, cancellationToken);
        if (user is null)
        {
            return Result.Failure<ScheduleDto>($"User with ID '{command.UserId}' not found.");
        }

        if (!Enum.TryParse<WorldType>(command.World, ignoreCase: true, out var world))
        {
            return Result.Failure<ScheduleDto>($"Invalid world value '{command.World}'.");
        }

        if (!Enum.TryParse<ScheduleType>(command.ScheduleType, ignoreCase: true, out var scheduleType))
        {
            return Result.Failure<ScheduleDto>($"Invalid schedule type value '{command.ScheduleType}'.");
        }

        var schedule = new ScheduleEntity
        {
            Id = Guid.NewGuid(),
            UserGuid = command.UserId,
            Name = command.Name,
            CreationDate = DateTimeOffset.UtcNow,
            World = world,
            ScheduleType = scheduleType,
            Enemies = new List<TribeInfo>()
        };


        // Handle enemies if provided
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

        await scheduleRepository.AddAsync(schedule, cancellationToken);

        return Result.Success(IScheduleMapper.ToDto(schedule));
    }
}


