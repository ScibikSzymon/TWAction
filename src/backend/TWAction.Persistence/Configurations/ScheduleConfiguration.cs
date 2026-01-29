using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
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
        builder.Property(x => x.Enemies)
               .HasConversion(
                   v => JsonSerializer.Serialize(v, options),
                   v => JsonSerializer.Deserialize<List<TribeInfo>>(v, options) ?? new List<TribeInfo>());

        builder.HasOne<UserEntity>()
               .WithMany(u => u.Schedules)
               .HasForeignKey(s => s.UserGuid)
               .OnDelete(DeleteBehavior.Cascade);
    }
}


