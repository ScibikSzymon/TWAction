using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TWAction.Domain.Schedules;

namespace TWAction.Persistence.Configurations;

public sealed class TroopsStateConfiguration : IEntityTypeConfiguration<TroopsStateEntity>
{
    public void Configure(EntityTypeBuilder<TroopsStateEntity> builder)
    {
        builder.ToTable("TroopsStates");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ScheduleId).IsRequired();
        builder.Property(x => x.CompressedData).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();

        // Unique constraint: one troops state per schedule
        builder.HasIndex(x => x.ScheduleId).IsUnique();

        // Relationship with Schedule
        builder.HasOne<ScheduleEntity>()
               .WithMany()
               .HasForeignKey(t => t.ScheduleId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
