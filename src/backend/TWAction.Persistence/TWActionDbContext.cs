using Microsoft.EntityFrameworkCore;
using TWAction.Domain.Users;
using TWAction.Domain.Schedules;
using TWAction.Persistence.Configurations;

namespace TWAction.Persistence;

public class TWActionDbContext(DbContextOptions<TWActionDbContext> options) : DbContext(options)
{
    public DbSet<UserEntity> Users { get; set; } = null!;

    public DbSet<UserSessionEntity> UserSessions { get; set; } = null!;

    public DbSet<ScheduleEntity> Schedules { get; set; } = null!;

    public DbSet<TroopsStateEntity> TroopsStates { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new UserSessionConfiguration());
        modelBuilder.ApplyConfiguration(new ScheduleConfiguration());
        modelBuilder.ApplyConfiguration(new TroopsStateConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}
