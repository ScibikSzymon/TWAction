using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TWAction.Domain.Users;

namespace TWAction.Persistence.Configurations;

public sealed class SubscriptionPlanLimitsConfiguration
    : IEntityTypeConfiguration<SubscriptionPlanLimitsEntity>
{
    public void Configure(EntityTypeBuilder<SubscriptionPlanLimitsEntity> builder)
    {
        builder.ToTable("SubscriptionPlanLimits");
        builder.HasKey(x => x.SubscriptionTier);
        builder.Property(x => x.SubscriptionTier).HasConversion<string>();
        builder.Property(x => x.TroopsUploadWindowHours).IsRequired();
    }
}
