using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MaritimeEdge.Data;
using MaritimeEdge.Services;
using MaritimeEdge.Services.Logbooks;

namespace MaritimeEdge
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configure port - Listen on all network interfaces for mobile access
            // Can be overridden by command line: dotnet run --urls "http://0.0.0.0:5005"
            builder.WebHost.UseUrls("http://0.0.0.0:5005");

            // Add services to the container
            var connectionString = builder.Configuration.GetValue<string>("Database:ConnectionString");
            
            builder.Services.AddDbContext<EdgeDbContext>(options =>
                options.UseNpgsql(connectionString)
            );

            // Add HttpClient for SignalK
            builder.Services.AddHttpClient("SignalK", client =>
            {
                client.Timeout = TimeSpan.FromSeconds(30);
                client.DefaultRequestHeaders.Add("User-Agent", "MaritimeEdge/1.0");
            });

            // Add Memory Cache for performance optimization
            builder.Services.AddMemoryCache();

            // Add Business Services
            builder.Services.AddScoped<FuelAnalyticsService>();
            builder.Services.AddScoped<ISignalKHttpClient, SignalKHttpClient>();
            builder.Services.AddScoped<TaskManagementService>();
            builder.Services.AddScoped<IReportingService, ReportingService>();
            builder.Services.AddScoped<IAggregateReportService, AggregateReportService>();
            builder.Services.AddScoped<ISyncService, SyncService>();
            builder.Services.AddScoped<IWatchkeepingService, WatchkeepingService>();
            builder.Services.AddScoped<IDeckLogbookService, DeckLogbookService>();
            builder.Services.AddScoped<IEngineLogbookService, EngineLogbookService>();
            builder.Services.AddScoped<IGarbageRecordService, GarbageRecordService>();
            builder.Services.AddScoped<IBallastWaterService, BallastWaterService>();
            builder.Services.AddScoped<IOilRecordService, OilRecordService>();
            builder.Services.AddScoped<MaterialReceiptService>();

            // Add Background Services
            builder.Services.AddHostedService<TelemetrySimulatorService>();
            builder.Services.AddHostedService<SignalKDataCollectorService>();
            builder.Services.AddHostedService<DataCleanupService>();
            builder.Services.AddHostedService<SyncBackgroundWorker>();

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
            // TEMPORARILY DISABLED due to migration conflicts
            // Tables are created via SQL script: create-equipment-tables.sql
            /*
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
            */

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