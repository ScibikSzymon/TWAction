using TWAction.Application.Common;
using TWAction.Application.Schedules.DTOs;
using TWAction.Application.Schedules.Interfaces;
using TWAction.Domain.Schedules;

namespace TWAction.Application.Schedules.Commands;

public sealed record SaveNobleBudgetsCommand(Guid ScheduleId, List<PlayerBudgetItem> PlayerBudgets);

public class SaveNobleBudgetsHandler(
    IScheduleRepository scheduleRepository,
    INobleBudgetRepository nobleBudgetRepository)
{
    public async Task<Result<IReadOnlyList<NobleBudgetDto>>> Handle(SaveNobleBudgetsCommand command, CancellationToken cancellationToken = default)
    {
        var schedule = await scheduleRepository.GetByIdAsync(command.ScheduleId, cancellationToken);
        if (schedule is null)
        {
            return Result.Failure<IReadOnlyList<NobleBudgetDto>>($"Schedule with ID '{command.ScheduleId}' not found.");
        }

        if (command.PlayerBudgets == null || !command.PlayerBudgets.Any())
        {
            return Result.Failure<IReadOnlyList<NobleBudgetDto>>("Player budgets list cannot be empty.");
        }

        // Validate budgets
        foreach (var playerBudget in command.PlayerBudgets)
        {
            if (playerBudget.Budget < 0)
            {
                return Result.Failure<IReadOnlyList<NobleBudgetDto>>($"Budget for player {playerBudget.PlayerId} cannot be negative.");
            }
        }

        var now = DateTimeOffset.UtcNow;
        var nobleBudgets = command.PlayerBudgets.Select(pb => new NobleBudgetEntity
        {
            Id = Guid.NewGuid(),
            ScheduleId = command.ScheduleId,
            PlayerId = pb.PlayerId,
            Budget = pb.Budget,
            CreatedAt = now,
            UpdatedAt = now
        }).ToList();

        await nobleBudgetRepository.UpsertManyAsync(nobleBudgets, cancellationToken);

        // Fetch all budgets for this schedule to return
        var allBudgets = await nobleBudgetRepository.GetByScheduleIdAsync(command.ScheduleId, cancellationToken);

        var dtos = allBudgets.Select(nb => new NobleBudgetDto
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
