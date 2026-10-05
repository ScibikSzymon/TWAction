using TWAction.Application.Common;
using TWAction.Application.TargetGroups.Interfaces;

namespace TWAction.Application.TargetGroups.Commands;

public sealed record DeleteTargetGroupCommand(Guid GroupId, Guid ScheduleId);

public sealed class DeleteTargetGroupHandler(ITargetGroupRepository repository)
{
    public async Task<Result> Handle(DeleteTargetGroupCommand command, CancellationToken ct = default)
    {
        var group = await repository.GetByIdAsync(command.GroupId, ct);

        if (group is null || group.ScheduleId != command.ScheduleId)
        {
            return Result.Failure("Target group not found.");
        }

        await repository.DeleteAsync(group, ct);
        return Result.Success();
    }
}
