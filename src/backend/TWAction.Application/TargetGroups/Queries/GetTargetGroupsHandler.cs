using TWAction.Application.Common;
using TWAction.Application.TargetGroups.DTOs;
using TWAction.Application.TargetGroups.Interfaces;
using TWAction.Application.TargetGroups.Mappers;

namespace TWAction.Application.TargetGroups.Queries;

/// <summary>Returns all target groups belonging to the given schedule, ordered by name.</summary>
public sealed record GetTargetGroupsQuery(Guid ScheduleId);

public sealed class GetTargetGroupsHandler(ITargetGroupRepository repository)
{
    public async Task<Result<IEnumerable<TargetGroupDto>>> Handle(GetTargetGroupsQuery query, CancellationToken ct = default)
    {
        var groups = await repository.GetAllAsync(query.ScheduleId, ct);
        var dtos = groups.OrderBy(g => g.Name).Select(g => g.ToDto());
        return Result.Success<IEnumerable<TargetGroupDto>>(dtos);
    }
}
