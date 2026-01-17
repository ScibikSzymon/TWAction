using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TWAction.Persistence;

/// <summary>
/// Design-time factory for creating TWActionDbContext instances during migrations.
/// This factory is used by EF Core tools (dotnet ef) to create the DbContext without running the application.
/// </summary>
public sealed class TWActionDbContextFactory : IDesignTimeDbContextFactory<TWActionDbContext>
{
    public TWActionDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<TWActionDbContext>();
        
        // Use a connection string for design-time operations
        // This is only used for migrations and won't affect runtime behavior
        optionsBuilder.UseNpgsql(
            "Host=localhost;Database=twaction_design;Username=postgres;Password=postgres",
            b => b.MigrationsAssembly("TWAction.Persistence"));
        
        return new TWActionDbContext(optionsBuilder.Options);
    }
}