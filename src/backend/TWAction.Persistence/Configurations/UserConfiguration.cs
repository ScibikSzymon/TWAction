using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TWAction.Domain.Users;

namespace TWAction.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<UserEntity>
{
    public void Configure(EntityTypeBuilder<UserEntity> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Email).IsRequired();
        builder.Property(x => x.Provider).IsRequired();
        builder.Property(x => x.Role)
            .IsRequired()
            .HasConversion<string>();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.HasIndex(x => new { x.Email, x.Provider }).IsUnique();
    }
}
