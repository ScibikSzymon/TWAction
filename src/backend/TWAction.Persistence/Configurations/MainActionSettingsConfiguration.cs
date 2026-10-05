using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TWAction.Domain.Schedules;
using TWAction.Domain.Settings;

namespace TWAction.Persistence.Configurations;

public sealed class MainActionSettingsConfiguration : IEntityTypeConfiguration<MainActionSettings>
{
    public void Configure(EntityTypeBuilder<MainActionSettings> builder)
    {
        var nobleBudgetsComparer = new ValueComparer<Dictionary<int, uint>>(
            (left, right) =>
                ReferenceEquals(left, right) ||
                (left != null && right != null &&
                 left.Count == right.Count &&
                 left.All(item => right.ContainsKey(item.Key) && right[item.Key] == item.Value)),
            value => value == null
                ? 0
                : value
                    .OrderBy(item => item.Key)
                    .Aggregate(0, (current, item) => HashCode.Combine(current, item.Key, item.Value)),
            value => value == null ? new Dictionary<int, uint>() : value.ToDictionary(item => item.Key, item => item.Value));

        builder.ToTable("MainActionSettings");
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.ScheduleId).IsRequired();
        builder.Property(x => x.MinDepartureTime).IsRequired();
        builder.Property(x => x.SkipNightSendings).IsRequired();
        builder.Property(x => x.MaxNobleDistance).IsRequired();
        builder.Property(x => x.ActionDate).IsRequired();

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
            .HasColumnType("jsonb")
            .Metadata.SetValueComparer(nobleBudgetsComparer);

        // Unique constraint - one settings per schedule
        builder.HasIndex(x => x.ScheduleId).IsUnique();

        // Cascade delete when schedule is deleted
        builder.HasOne<ScheduleEntity>()
               .WithMany()
               .HasForeignKey(x => x.ScheduleId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
