using TWAction.Application.Common;
using TWAction.Application.TargetGroups.DTOs;
using TWAction.Application.TargetGroups.Interfaces;
using TWAction.Application.TargetGroups.Mappers;

namespace TWAction.Application.TargetGroups.Queries;

/// <summary>Returns a single target group, verifying it belongs to the requested schedule.</summary>
public sealed record GetTargetGroupByIdQuery(Guid GroupId, Guid ScheduleId);

public sealed class GetTargetGroupByIdHandler(ITargetGroupRepository repository)
{
    public async Task<Result<TargetGroupDto>> Handle(GetTargetGroupByIdQuery query, CancellationToken ct = default)
    {
        var group = await repository.GetByIdAsync(query.GroupId, ct);

        if (group is null || group.ScheduleId != query.ScheduleId)
        {
            return Result.Failure<TargetGroupDto>("Target group not found.");
        }

        return Result.Success(group.ToDto());
    }
}
