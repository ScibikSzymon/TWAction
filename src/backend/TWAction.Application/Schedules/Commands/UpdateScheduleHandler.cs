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

public class UpdateScheduleHandler
{
    private readonly IScheduleRepository _scheduleRepository;

    public UpdateScheduleHandler(IScheduleRepository scheduleRepository)
    {
        _scheduleRepository = scheduleRepository;
    }

    public async Task<Result<ScheduleDto>> Handle(UpdateScheduleCommand command, CancellationToken cancellationToken = default)
    {
        var schedule = await _scheduleRepository.GetByIdAsync(command.ScheduleId, cancellationToken);

        if (schedule is null)
        {
            return Result.Failure<ScheduleDto>($"Schedule with ID '{command.ScheduleId}' not found.");
        }

        if (string.IsNullOrWhiteSpace(command.Name))
        {
            return Result.Failure<ScheduleDto>("Schedule name cannot be empty.");
        }

        schedule.Name = command.Name;
        schedule.World = Enum.Parse<WorldType>(command.World);
        schedule.ScheduleType = Enum.Parse<ScheduleType>(command.ScheduleType);

        await _scheduleRepository.UpdateAsync(schedule, cancellationToken);

        return Result.Success(IScheduleMapper.ToDto(schedule));
    }
}
