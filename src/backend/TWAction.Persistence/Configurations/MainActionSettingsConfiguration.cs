using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TWAction.Domain.Schedules;
using TWAction.Domain.Settings;

namespace TWAction.Persistence.Configurations;

public sealed class MainActionSettingsConfiguration : IEntityTypeConfiguration<MainActionSettings>
{
    public void Configure(EntityTypeBuilder<MainActionSettings> builder)
    {
        builder.ToTable("MainActionSettings");
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.ScheduleId).IsRequired();
        builder.Property(x => x.MinDepartureTime).IsRequired();
        builder.Property(x => x.SkipNightSendings).IsRequired();
        builder.Property(x => x.MaxNobleDistance).IsRequired();

        // Configure owned entities for nested settings
        builder.OwnsOne(x => x.OffSettings, offBuilder =>
        {
            offBuilder.Property(o => o.MinOffUnits).IsRequired();
            offBuilder.Property(o => o.MinDistanceFromFront).IsRequired();
        });

        builder.OwnsOne(x => x.CatasSettings, catasBuilder =>
        {
            catasBuilder.Property(c => c.MinCatasNumber).IsRequired();
            catasBuilder.Property(c => c.MinDistanceFromFront).IsRequired();
            catasBuilder.Property(c => c.MaxOffUnits).IsRequired();
        });

        builder.OwnsOne(x => x.FakeOffSettings, fakeOffBuilder =>
        {
            fakeOffBuilder.Property(f => f.MinOffUnits).IsRequired();
            fakeOffBuilder.Property(f => f.MinDistanceFromFront).IsRequired();
        });

        builder.OwnsOne(x => x.FakeDeffSettings, fakeDeffBuilder =>
        {
            fakeDeffBuilder.Property(f => f.MaxOffUnits).IsRequired();
            fakeDeffBuilder.Property(f => f.MinDistanceFromFront).IsRequired();
        });

        builder.OwnsOne(x => x.NobleSettings, nobleBuilder =>
        {
            nobleBuilder.Property(n => n.MinDistanceFromFront).IsRequired();
            nobleBuilder.Property(n => n.MinOffUnitsForOffNoble).IsRequired();
            nobleBuilder.Property(n => n.MinOffUnitsForFakeOffNoble).IsRequired();
            nobleBuilder.Property(n => n.MaxOffUnitsForDefNoble).IsRequired();
            nobleBuilder.Property(n => n.MinDeffUnitsForDefNoble).IsRequired();
        });

        // Store PlayerNobleBudgets as JSON column
        builder.Property(x => x.PlayerNobleBudgets)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<Dictionary<int, uint>>(v, (JsonSerializerOptions?)null) ?? new Dictionary<int, uint>())
            .HasColumnType("jsonb");

        // Unique constraint - one settings per schedule
        builder.HasIndex(x => x.ScheduleId).IsUnique();

        // Cascade delete when schedule is deleted
        builder.HasOne<ScheduleEntity>()
               .WithMany()
               .HasForeignKey(x => x.ScheduleId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
