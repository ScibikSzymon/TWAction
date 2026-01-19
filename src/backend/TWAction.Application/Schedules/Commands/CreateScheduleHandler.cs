using TWAction.Application.Common;
using TWAction.Application.Mappers;
using TWAction.Application.Schedules.DTOs;
using TWAction.Application.Schedules.Interfaces;
using TWAction.Application.Users.Interfaces;
using TWAction.Domain.Schedules;

namespace TWAction.Application.Schedules.Commands;

public sealed record CreateScheduleCommand(
    Guid UserId,
    string Name,
    string World,
    string ScheduleType
);

public class CreateScheduleHandler(IScheduleRepository scheduleRepository, IUserRepository userRepository)
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

        var schedule = new ScheduleEntity
        {
            Id = Guid.NewGuid(),
            UserGuid = command.UserId,
            Name = command.Name,
            CreationDate = DateTime.UtcNow,
            World = Enum.Parse<WorldType>(command.World),
            ScheduleType = Enum.Parse<ScheduleType>(command.ScheduleType)
        };

        await scheduleRepository.AddAsync(schedule, cancellationToken);

        return Result.Success(IScheduleMapper.ToDto(schedule));
    }
}
