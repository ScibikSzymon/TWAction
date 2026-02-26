using TWAction.Application.Common;
using TWAction.Application.Mappers;
using TWAction.Application.Schedules.DTOs;
using TWAction.Application.Schedules.Interfaces;

namespace TWAction.Application.Schedules.Queries;

public sealed record GetScheduleByIdQuery(Guid ScheduleId);

public class GetScheduleByIdHandler(IScheduleRepository scheduleRepository)
{
    public async Task<Result<ScheduleDto>> Handle(GetScheduleByIdQuery query, CancellationToken cancellationToken = default)
    {
        var schedule = await scheduleRepository.GetByIdAsync(query.ScheduleId, cancellationToken);

        if (schedule is null)
        {
            return Result.Failure<ScheduleDto>($"Schedule with ID '{query.ScheduleId}' not found.");
        }

        return Result.Success(IScheduleMapper.ToDto(schedule));
    }
}
