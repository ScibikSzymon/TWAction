using Microsoft.EntityFrameworkCore;
using TWAction.Domain.Entities;
using TWAction.Persistence.Configurations;

namespace TWAction.Persistence
{
    public class TWActionDbContext : DbContext
    {
        public TWActionDbContext(DbContextOptions<TWActionDbContext> options) : base(options) { }

        public DbSet<ExampleEntity> ExampleEntities { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new ExampleEntityConfiguration());
            base.OnModelCreating(modelBuilder);
        }
    }
}
