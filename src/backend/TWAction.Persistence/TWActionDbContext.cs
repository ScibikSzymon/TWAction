using Microsoft.EntityFrameworkCore;
using TWAction.Domain.Entities;
using TWAction.Persistence.Configurations;

namespace TWAction.Persistence
{
    public class TWActionDbContext : DbContext
    {
        public TWActionDbContext(DbContextOptions<TWActionDbContext> options) : base(options) { }

        public DbSet<ExampleEntity> ExampleEntities { get; set; } = null!;

        public DbSet<User> Users { get; set; } = null!;

        public DbSet<UserSession> UserSessions { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new ExampleEntityConfiguration());
            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfiguration(new UserSessionConfiguration());
            base.OnModelCreating(modelBuilder);
        }
    }
}
