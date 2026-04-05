using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace ProductApi.Data
{
    /// <summary>
    /// Design-time factory for creating DbContext instances.
    /// This is used by Entity Framework Core CLI tools (dotnet ef) to:
    /// 1. Generate migrations without running the application
    /// 2. Allow teammates to pull code and run migrations immediately
    /// 3. Avoid runtime dependency injection container complications during migration generation
    /// 
    /// Usage: When you modify AppDbContext, run:
    ///   dotnet ef migrations add "MigrationName" -p . -s .
    /// </summary>
    public class AppDbContextDesignTimeFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            // Try to load configuration from environment or appsettings.json
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables()
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? "Host=localhost;Port=5432;Database=maritime_shore_dev;Username=postgres;Password=postgres";

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseNpgsql(connectionString)
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
