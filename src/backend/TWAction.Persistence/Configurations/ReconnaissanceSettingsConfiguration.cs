using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TWAction.Domain.Schedules;
using TWAction.Domain.Settings;

namespace TWAction.Persistence.Configurations;

public sealed class ReconnaissanceSettingsConfiguration : IEntityTypeConfiguration<ReconnaissanceSettings>
{
    public void Configure(EntityTypeBuilder<ReconnaissanceSettings> builder)
    {
        builder.ToTable("ReconnaissanceSettings");
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.ScheduleId).IsRequired();
        builder.Property(x => x.MinDepartureTime).IsRequired();
        builder.Property(x => x.MinArrivalTime).IsRequired();
        builder.Property(x => x.MaxArrivalTime).IsRequired();
        builder.Property(x => x.MinDistanceToFront).IsRequired();
        builder.Property(x => x.MinSpyCount).IsRequired();
        builder.Property(x => x.MaxPopulationInSourceVillage).IsRequired();
        builder.Property(x => x.SkipNightSendings).IsRequired();

        // Unique constraint - one settings per schedule
        builder.HasIndex(x => x.ScheduleId).IsUnique();

        // Cascade delete when schedule is deleted
        builder.HasOne<ScheduleEntity>()
               .WithMany()
               .HasForeignKey(x => x.ScheduleId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
