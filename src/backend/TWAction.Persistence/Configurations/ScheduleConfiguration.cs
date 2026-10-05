using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json;
using TWAction.Domain.Schedules;
using TWAction.Domain.Tribes;
using TWAction.Domain.Users;

namespace TWAction.Persistence.Configurations;

public sealed class ScheduleConfiguration : IEntityTypeConfiguration<ScheduleEntity>
{
    public void Configure(EntityTypeBuilder<ScheduleEntity> builder)
    {
        builder.ToTable("Schedules");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.UserGuid).IsRequired();
        builder.Property(x => x.Name).IsRequired();
        builder.Property(x => x.CreationDate).IsRequired();
        builder.Property(x => x.World).IsRequired();
        builder.Property(x => x.ScheduleType).IsRequired();

        // Store enemies as JSON
        var options = new JsonSerializerOptions();
        var enemiesComparer = new ValueComparer<List<TribeInfo>>(
            (left, right) =>
                ReferenceEquals(left, right) ||
                (left != null && right != null && left.SequenceEqual(right, new TribeInfoComparer())),
            value => value == null
                ? 0
                : value.Aggregate(0, (current, item) => HashCode.Combine(current, item.TribalWarsId, item.Name, item.Short, item.VillagesCount)),
            value => value == null ? new List<TribeInfo>() : value.Select(item => new TribeInfo
            {
                TribalWarsId = item.TribalWarsId,
                Name = item.Name,
                Short = item.Short,
                VillagesCount = item.VillagesCount
            }).ToList());

        builder.Property(x => x.Enemies)
               .HasConversion(
                   v => JsonSerializer.Serialize(v, options),
                   v => JsonSerializer.Deserialize<List<TribeInfo>>(v, options) ?? new List<TribeInfo>())
               .Metadata.SetValueComparer(enemiesComparer);


        builder.HasOne<UserEntity>()
               .WithMany(u => u.Schedules)
               .HasForeignKey(s => s.UserGuid)
               .OnDelete(DeleteBehavior.Cascade);
    }

    private sealed class TribeInfoComparer : IEqualityComparer<TribeInfo>
    {
        public bool Equals(TribeInfo? x, TribeInfo? y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (x is null || y is null) return false;
            return x.TribalWarsId == y.TribalWarsId &&
                   x.Name == y.Name &&
                   x.Short == y.Short &&
                   x.VillagesCount == y.VillagesCount;
        }

        public int GetHashCode(TribeInfo obj)
            => HashCode.Combine(obj.TribalWarsId, obj.Name, obj.Short, obj.VillagesCount);
    }
}


