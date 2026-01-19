using TWAction.Application.Common;
using TWAction.Application.Mappers;
using TWAction.Application.Schedules.DTOs;
using TWAction.Application.Schedules.Interfaces;

namespace TWAction.Application.Schedules.Queries;

public sealed record GetAllSchedulesQuery(Guid UserId);

public class GetAllSchedulesHandler
{
    private readonly IScheduleRepository _scheduleRepository;

    public GetAllSchedulesHandler(IScheduleRepository scheduleRepository)
    {
        _scheduleRepository = scheduleRepository;
    }

    public async Task<Result<IEnumerable<ScheduleDto>>> Handle(GetAllSchedulesQuery query, CancellationToken cancellationToken = default)
    {
        var schedules = await _scheduleRepository.ListByUserIdAsync(query.UserId, cancellationToken);
        var scheduleDtos = schedules.Select(IScheduleMapper.ToDto).ToList();
        return Result.Success<IEnumerable<ScheduleDto>>(scheduleDtos);
    }
}
