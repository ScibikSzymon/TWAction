using TWAction.Application.Common;
using TWAction.Application.Schedules.DTOs;
using TWAction.Application.Schedules.Interfaces;

namespace TWAction.Application.Schedules.Queries;

public sealed record GetNobleBudgetsQuery(Guid ScheduleId);

public class GetNobleBudgetsHandler(
    IScheduleRepository scheduleRepository,
    INobleBudgetRepository nobleBudgetRepository)
{
    public async Task<Result<IReadOnlyList<NobleBudgetDto>>> Handle(GetNobleBudgetsQuery query, CancellationToken cancellationToken = default)
    {
        var schedule = await scheduleRepository.GetByIdAsync(query.ScheduleId, cancellationToken);
        if (schedule is null)
        {
            return Result.Failure<IReadOnlyList<NobleBudgetDto>>($"Schedule with ID '{query.ScheduleId}' not found.");
        }

        var budgets = await nobleBudgetRepository.GetByScheduleIdAsync(query.ScheduleId, cancellationToken);

        var dtos = budgets.Select(nb => new NobleBudgetDto
        {
            Id = nb.Id,
            ScheduleId = nb.ScheduleId,
            PlayerId = nb.PlayerId,
            Budget = nb.Budget,
            CreatedAt = nb.CreatedAt,
            UpdatedAt = nb.UpdatedAt
        }).ToList();

        return Result.Success<IReadOnlyList<NobleBudgetDto>>(dtos);
    }
}
