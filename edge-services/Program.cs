using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MaritimeEdge.Data;
using MaritimeEdge.Services;

namespace MaritimeEdge
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configure port
            builder.WebHost.UseUrls("http://localhost:5001");

            // Add services to the container
            var connectionString = builder.Configuration.GetValue<string>("Database:ConnectionString");
            
            builder.Services.AddDbContext<EdgeDbContext>(options =>
                options.UseNpgsql(connectionString)
            );

            // Add Business Services
            builder.Services.AddScoped<FuelAnalyticsService>();

            // Add Background Services
            builder.Services.AddHostedService<TelemetrySimulatorService>();
            builder.Services.AddHostedService<DataCleanupService>();

            // Add Controllers
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
                });
            
            // Add CORS for frontend-edge (support both port 3001 and 3002)
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                {
                    policy.SetIsOriginAllowed(origin =>
                    {
                        if (string.IsNullOrWhiteSpace(origin)) return false;
                        
                        // Allow localhost on any port
                        if (origin.StartsWith("http://localhost:") || origin.StartsWith("https://localhost:"))
                            return true;
                        
                        // Allow local network IPs (192.168.x.x)
                        if (origin.StartsWith("http://192.168.") || origin.StartsWith("https://192.168."))
                            return true;
                        
                        // Allow 172.x.x.x range (Docker/VM networks)
                        if (origin.StartsWith("http://172.") || origin.StartsWith("https://172."))
                            return true;
                        
                        return false;
                    })
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
                });
            });

            // Add Swagger
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new() { Title = "Maritime Edge API", Version = "v1" });
            });

            var app = builder.Build();

            // Initialize database with migrations
            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<EdgeDbContext>();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                
                try
                {
                    logger.LogInformation("Applying database migrations...");
                    await dbContext.Database.MigrateAsync();
                    logger.LogInformation("Database migrations applied successfully");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error applying database migrations");
                    throw;
                }
            }

            // Configure the HTTP request pipeline
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Maritime Edge API v1");
                c.RoutePrefix = "swagger";
            });

            app.UseCors("AllowFrontend");
            app.UseRouting();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}