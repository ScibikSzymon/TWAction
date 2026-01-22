using TWAction.Application.Common;
using TWAction.Application.Mappers;
using TWAction.Application.Schedules.DTOs;
using TWAction.Application.Schedules.Interfaces;
using TWAction.Domain.Schedules;

namespace TWAction.Application.Schedules.Commands;

public sealed record UpdateScheduleCommand(
    Guid ScheduleId,
    string Name,
    string World,
    string ScheduleType
);

public class UpdateScheduleHandler(IScheduleRepository scheduleRepository)
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

        schedule.World = world;
        schedule.ScheduleType = scheduleType;
        await scheduleRepository.UpdateAsync(schedule, cancellationToken);

        return Result.Success(IScheduleMapper.ToDto(schedule));
    }
}
