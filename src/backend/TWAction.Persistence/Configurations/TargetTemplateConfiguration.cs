using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TWAction.Domain.Templates;
using TWAction.Domain.Users;

namespace TWAction.Persistence.Configurations;

public sealed class TargetTemplateConfiguration : IEntityTypeConfiguration<TargetTemplate>
{
    public void Configure(EntityTypeBuilder<TargetTemplate> builder)
    {
        builder.ToTable("TargetTemplates");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.IsDefault)
            .IsRequired();

        builder.Property(t => t.UserId);

        // Store the list of waves as a JSON blob for simplicity —
        // waves are always loaded with the template and never queried individually.
        builder.Property(t => t.Waves)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<TemplateWave>>(v, (JsonSerializerOptions?)null) ?? new List<TemplateWave>())
            .HasColumnType("jsonb");

        // Index for efficient per-user lookups.
        builder.HasIndex(t => t.UserId);

        // Cascade delete a user's templates when the user account is removed.
        // Default templates have no UserId (null) and are unaffected.
        builder.HasOne<UserEntity>()
               .WithMany()
               .HasForeignKey(t => t.UserId)
               .IsRequired(false)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
