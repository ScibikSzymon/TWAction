using Microsoft.EntityFrameworkCore;
using TWAction.Domain.Users;
using TWAction.Domain.Schedules;
using TWAction.Domain.Settings;
using TWAction.Domain.ReconnaissanceActions;
using TWAction.Persistence.Configurations;

namespace TWAction.Persistence;

public class TWActionDbContext(DbContextOptions<TWActionDbContext> options) : DbContext(options)
{
    public DbSet<UserEntity> Users { get; set; } = null!;

    public DbSet<UserSessionEntity> UserSessions { get; set; } = null!;

    public DbSet<ScheduleEntity> Schedules { get; set; } = null!;

    public DbSet<TroopsStateEntity> TroopsStates { get; set; } = null!;

    public DbSet<ReconnaissanceSettings> ReconnaissanceSettings { get; set; } = null!;

    public DbSet<AttackCommandEntity> AttackCommands { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new UserSessionConfiguration());
        modelBuilder.ApplyConfiguration(new ScheduleConfiguration());
        modelBuilder.ApplyConfiguration(new TroopsStateConfiguration());
        modelBuilder.ApplyConfiguration(new ReconnaissanceSettingsConfiguration());
        modelBuilder.ApplyConfiguration(new AttackCommandConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}

