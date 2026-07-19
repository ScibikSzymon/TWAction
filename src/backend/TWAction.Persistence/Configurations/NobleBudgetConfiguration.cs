using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TWAction.Domain.Schedules;

namespace TWAction.Persistence.Configurations;

public sealed class NobleBudgetConfiguration : IEntityTypeConfiguration<NobleBudgetEntity>
{
    public void Configure(EntityTypeBuilder<NobleBudgetEntity> builder)
    {
        builder.ToTable("NobleBudgets");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ScheduleId).IsRequired();
        builder.Property(x => x.PlayerId).IsRequired();
        builder.Property(x => x.Budget).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();

        // Unique constraint: one budget per player per schedule
        builder.HasIndex(x => new { x.ScheduleId, x.PlayerId }).IsUnique();

        // Relationship with Schedule
        builder.HasOne<ScheduleEntity>()
               .WithMany()
               .HasForeignKey(nb => nb.ScheduleId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
