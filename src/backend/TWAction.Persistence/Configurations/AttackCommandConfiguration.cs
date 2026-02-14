using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TWAction.Domain.ReconnaissanceActions;

namespace TWAction.Persistence.Configurations;

public sealed class AttackCommandConfiguration : IEntityTypeConfiguration<AttackCommandEntity>
{
    public void Configure(EntityTypeBuilder<AttackCommandEntity> builder)
    {
        builder.ToTable("AttackCommands");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.ScheduleId)
            .IsRequired();

        builder.Property(e => e.MinDepartureTime)
            .IsRequired();

        builder.Property(e => e.MaxDepartureTime)
            .IsRequired();

        builder.Property(e => e.MinArrivalTime)
            .IsRequired();

        builder.Property(e => e.MaxArrivalTime)
            .IsRequired();

        builder.Property(e => e.SourceVillageId)
            .IsRequired();

        builder.Property(e => e.SourceX)
            .IsRequired();

        builder.Property(e => e.SourceY)
            .IsRequired();

        builder.Property(e => e.SourcePlayerId)
            .IsRequired();

        builder.Property(e => e.DestinationVillageId)
            .IsRequired();

        builder.Property(e => e.DestinationX)
            .IsRequired();

        builder.Property(e => e.DestinationY)
            .IsRequired();

        builder.Property(e => e.DestinationPlayerId)
            .IsRequired();

        builder.Property(e => e.CommandType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        builder.HasIndex(e => e.ScheduleId);
        builder.HasIndex(e => e.CreatedAt);
    }
}
