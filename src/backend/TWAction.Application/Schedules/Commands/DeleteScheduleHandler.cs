using TWAction.Application.Common;
using TWAction.Application.Schedules.Interfaces;

namespace TWAction.Application.Schedules.Commands;

public sealed record DeleteScheduleCommand(Guid ScheduleId);

public class DeleteScheduleHandler
{
    private readonly IScheduleRepository _scheduleRepository;

    public DeleteScheduleHandler(IScheduleRepository scheduleRepository)
    {
        _scheduleRepository = scheduleRepository;
    }

    public async Task<Result> Handle(DeleteScheduleCommand command, CancellationToken cancellationToken = default)
    {
        var schedule = await _scheduleRepository.GetByIdAsync(command.ScheduleId, cancellationToken);

        if (schedule is null)
        {
            return Result.Failure($"Schedule with ID '{command.ScheduleId}' not found.");
        }

        await _scheduleRepository.DeleteByIdAsync(command.ScheduleId, cancellationToken);

        return Result.Success();
    }
}
