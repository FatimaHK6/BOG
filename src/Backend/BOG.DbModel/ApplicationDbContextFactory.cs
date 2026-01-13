using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BOG.DbModel;

/// <summary>
/// Design-time DbContext factory for EF Core migrations.
/// Allows EF Core tools to create a DbContext instance without a full DI container.
/// Implements IDesignTimeDbContextFactory for design-time support.
/// </summary>
public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

        // Use LocalDB for migrations
        optionsBuilder.UseSqlServer(
            "Server=(localdb)\\mssqllocaldb;Database=BOG;Trusted_Connection=true;",
            sqlOptions => sqlOptions.MigrationsAssembly("BOG.DbModel")
        );

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
