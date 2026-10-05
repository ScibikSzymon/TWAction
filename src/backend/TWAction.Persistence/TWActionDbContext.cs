using Microsoft.EntityFrameworkCore;
using TWAction.Domain.Users;
using TWAction.Domain.Schedules;
using TWAction.Domain.Settings;
using TWAction.Domain.AttackCommands;
using TWAction.Domain.Templates;
using TWAction.Domain.TargetGroups;
using TWAction.Persistence.Configurations;

namespace TWAction.Persistence;

public class TWActionDbContext(DbContextOptions<TWActionDbContext> options) : DbContext(options)
{
    public DbSet<UserEntity> Users { get; set; } = null!;

    public DbSet<UserSessionEntity> UserSessions { get; set; } = null!;

    public DbSet<SubscriptionPlanLimitsEntity> SubscriptionPlanLimits { get; set; } = null!;

    public DbSet<ScheduleEntity> Schedules { get; set; } = null!;

    public DbSet<TroopsStateEntity> TroopsStates { get; set; } = null!;

    public DbSet<TroopsUploadEntity> TroopsUploads { get; set; } = null!;

    public DbSet<NobleBudgetEntity> NobleBudgets { get; set; } = null!;

    public DbSet<ReconnaissanceSettings> ReconnaissanceSettings { get; set; } = null!;

    public DbSet<MainActionSettings> MainActionSettings { get; set; } = null!;

    public DbSet<AttackCommandEntity> AttackCommands { get; set; } = null!;

    public DbSet<TargetTemplate> TargetTemplates { get; set; } = null!;

    public DbSet<TargetGroup> TargetGroups { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new UserSessionConfiguration());
        modelBuilder.ApplyConfiguration(new SubscriptionPlanLimitsConfiguration());
        modelBuilder.ApplyConfiguration(new ScheduleConfiguration());
        modelBuilder.ApplyConfiguration(new TroopsStateConfiguration());
        modelBuilder.Entity<TroopsUploadEntity>(entity =>
        {
            entity.ToTable("TroopsUploads");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.ScheduleId, x.UploadedAt });
            entity.HasOne<UserEntity>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<ScheduleEntity>().WithMany().HasForeignKey(x => x.ScheduleId).OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.ApplyConfiguration(new NobleBudgetConfiguration());
        modelBuilder.ApplyConfiguration(new ReconnaissanceSettingsConfiguration());
        modelBuilder.ApplyConfiguration(new MainActionSettingsConfiguration());
        modelBuilder.ApplyConfiguration(new AttackCommandConfiguration());
        modelBuilder.ApplyConfiguration(new TargetTemplateConfiguration());
        modelBuilder.ApplyConfiguration(new TargetGroupConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}

