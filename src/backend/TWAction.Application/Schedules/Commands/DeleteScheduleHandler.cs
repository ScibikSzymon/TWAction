using TWAction.Application.Common;
using TWAction.Application.Schedules.Interfaces;

namespace TWAction.Application.Schedules.Commands;

public sealed record DeleteScheduleCommand(Guid ScheduleId);

public class DeleteScheduleHandler(IScheduleRepository scheduleRepository)
{
    public async Task<Result> Handle(DeleteScheduleCommand command, CancellationToken cancellationToken = default)
    {
        var schedule = await scheduleRepository.GetByIdAsync(command.ScheduleId, cancellationToken);

        if (schedule is null)
        {
            return Result.Failure($"Schedule with ID '{command.ScheduleId}' not found.");
        }

        await scheduleRepository.DeleteByIdAsync(command.ScheduleId, cancellationToken);

        return Result.Success();
    }
}
