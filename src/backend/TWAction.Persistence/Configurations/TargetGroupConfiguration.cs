using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;
using TWAction.Domain.TargetGroups;
using TWAction.Domain.Templates;
using TWAction.Domain.Schedules;

namespace TWAction.Persistence.Configurations;

public sealed class TargetGroupConfiguration : IEntityTypeConfiguration<TargetGroup>
{
    public void Configure(EntityTypeBuilder<TargetGroup> builder)
    {
        var villageCoordinatesComparer = new ValueComparer<List<string>>(
            (left, right) =>
                ReferenceEquals(left, right) ||
                (left != null && right != null && left.SequenceEqual(right)),
            value => value == null
                ? 0
                : value.Aggregate(0, (current, coordinate) => HashCode.Combine(current, coordinate)),
            value => value == null ? new List<string>() : value.ToList());

        var wavesComparer = new ValueComparer<List<TemplateWave>>(
            (left, right) =>
                ReferenceEquals(left, right) ||
                (left != null && right != null &&
                 left.Count == right.Count &&
                 left.Zip(right).All(pair =>
                     pair.First.MinTime == pair.Second.MinTime &&
                     pair.First.MaxTime == pair.Second.MaxTime &&
                     pair.First.CommandNumber == pair.Second.CommandNumber &&
                     pair.First.CommandType == pair.Second.CommandType)),
            value => value == null
                ? 0
                : value.Aggregate(0, (current, wave) =>
                    HashCode.Combine(current, wave.MinTime, wave.MaxTime, wave.CommandNumber, wave.CommandType)),
            value => value == null
                ? new List<TemplateWave>()
                : value.Select(wave => new TemplateWave
                {
                    MinTime = wave.MinTime,
                    MaxTime = wave.MaxTime,
                    CommandNumber = wave.CommandNumber,
                    CommandType = wave.CommandType
                }).ToList());

        builder.ToTable("TargetGroups");
        builder.HasKey(g => g.Id);
        builder.Property(g => g.ScheduleId).IsRequired();
        builder.Property(g => g.Name).IsRequired().HasMaxLength(200);
        builder.Property(g => g.BaseTemplateName).HasMaxLength(200);

        // Village coordinate list stored as a JSON array — always loaded with the group
        builder.Property(g => g.VillageCoordinates)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>())
            .HasColumnType("jsonb")
            .Metadata.SetValueComparer(villageCoordinatesComparer);

        // Attack waves stored as a JSON array — same approach as TargetTemplateConfiguration
        builder.Property(g => g.Waves)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<TemplateWave>>(v, (JsonSerializerOptions?)null) ?? new List<TemplateWave>())
            .HasColumnType("jsonb")
            .Metadata.SetValueComparer(wavesComparer);

        builder.HasIndex(g => g.ScheduleId);

        // When the parent schedule is deleted all of its target groups are removed too
        builder.HasOne<ScheduleEntity>()
               .WithMany()
               .HasForeignKey(g => g.ScheduleId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
