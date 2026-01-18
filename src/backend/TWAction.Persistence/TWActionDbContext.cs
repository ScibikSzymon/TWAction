using Microsoft.EntityFrameworkCore;
using TWAction.Domain.Users;
using TWAction.Domain.Schedules;
using TWAction.Persistence.Configurations;

namespace TWAction.Persistence;

public class TWActionDbContext : DbContext
{
    public TWActionDbContext(DbContextOptions<TWActionDbContext> options) : base(options) { }


    public DbSet<UserEntity> Users { get; set; } = null!;

    public DbSet<UserSessionEntity> UserSessions { get; set; } = null!;

    public DbSet<ScheduleEntity> Schedules { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new UserSessionConfiguration());
        modelBuilder.ApplyConfiguration(new ScheduleConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}
