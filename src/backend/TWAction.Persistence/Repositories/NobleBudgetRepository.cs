using Microsoft.EntityFrameworkCore;
using TWAction.Application.Schedules.Interfaces;
using TWAction.Domain.Schedules;

namespace TWAction.Persistence.Repositories;

public class NobleBudgetRepository(TWActionDbContext db) : INobleBudgetRepository
{
    public async Task<IReadOnlyList<NobleBudgetEntity>> GetByScheduleIdAsync(Guid scheduleId, CancellationToken cancellationToken = default)
    {
        return await db.NobleBudgets
            .Where(nb => nb.ScheduleId == scheduleId)
            .ToListAsync(cancellationToken);
    }

    public async Task<NobleBudgetEntity?> GetByScheduleAndPlayerIdAsync(Guid scheduleId, int playerId, CancellationToken cancellationToken = default)
    {
        return await db.NobleBudgets
            .FirstOrDefaultAsync(nb => nb.ScheduleId == scheduleId && nb.PlayerId == playerId, cancellationToken);
    }

    public async Task<NobleBudgetEntity> CreateAsync(NobleBudgetEntity nobleBudget, CancellationToken cancellationToken = default)
    {
        await db.NobleBudgets.AddAsync(nobleBudget, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return nobleBudget;
    }

    public async Task<NobleBudgetEntity> UpdateAsync(NobleBudgetEntity nobleBudget, CancellationToken cancellationToken = default)
    {
        db.NobleBudgets.Update(nobleBudget);
        await db.SaveChangesAsync(cancellationToken);
        return nobleBudget;
    }

    public async Task UpsertManyAsync(IEnumerable<NobleBudgetEntity> nobleBudgets, CancellationToken cancellationToken = default)
    {
        var budgetsList = nobleBudgets.ToList();
        if (!budgetsList.Any()) return;

        var scheduleId = budgetsList.First().ScheduleId;
        var playerIds = budgetsList.Select(nb => nb.PlayerId).ToList();

        // Get existing budgets for these players
        var existingBudgets = await db.NobleBudgets
            .Where(nb => nb.ScheduleId == scheduleId && playerIds.Contains(nb.PlayerId))
            .ToListAsync(cancellationToken);

        var existingByPlayerId = existingBudgets.ToDictionary(nb => nb.PlayerId);

        foreach (var budget in budgetsList)
        {
            if (existingByPlayerId.TryGetValue(budget.PlayerId, out var existing))
            {
                // Update existing
                existing.Budget = budget.Budget;
                existing.UpdatedAt = DateTimeOffset.UtcNow;
                db.NobleBudgets.Update(existing);
            }
            else
            {
                // Create new
                await db.NobleBudgets.AddAsync(budget, cancellationToken);
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteByScheduleIdAsync(Guid scheduleId, CancellationToken cancellationToken = default)
    {
        var budgets = await db.NobleBudgets
            .Where(nb => nb.ScheduleId == scheduleId)
            .ToListAsync(cancellationToken);
        
        if (budgets.Count > 0)
        {
            db.NobleBudgets.RemoveRange(budgets);
            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
