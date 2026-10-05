using Microsoft.EntityFrameworkCore;
using TWAction.Application.Users.Interfaces;
using TWAction.Domain.Users;

namespace TWAction.Persistence.Repositories;

public sealed class SubscriptionPlanLimitsRepository(TWActionDbContext db)
    : ISubscriptionPlanLimitsRepository
{
    public async Task<SubscriptionPlanLimitsEntity> GetAsync(
        SubscriptionTier subscriptionTier,
        CancellationToken cancellationToken = default)
    {
        return await db.SubscriptionPlanLimits
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.SubscriptionTier == subscriptionTier, cancellationToken)
            ?? throw new InvalidOperationException(
                $"Limits for subscription plan '{subscriptionTier}' are not configured in the database.");
    }

    public async Task<IReadOnlyList<SubscriptionPlanLimitsEntity>> ListAsync(
        CancellationToken cancellationToken = default)
    {
        return await db.SubscriptionPlanLimits
            .AsNoTracking()
            .OrderBy(x => x.SubscriptionTier)
            .ToListAsync(cancellationToken);
    }
}
