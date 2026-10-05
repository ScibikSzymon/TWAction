using TWAction.Domain.Users;

namespace TWAction.Application.Users.Interfaces;

public interface ISubscriptionPlanLimitsRepository
{
    Task<SubscriptionPlanLimitsEntity> GetAsync(
        SubscriptionTier subscriptionTier,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SubscriptionPlanLimitsEntity>> ListAsync(
        CancellationToken cancellationToken = default);
}
