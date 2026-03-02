using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.FileProviders;
using System.Threading.RateLimiting;
using MaritimeEdge.Data;
using MaritimeEdge.Services.Core;
using MaritimeEdge.Services.Inventory;
using MaritimeEdge.Services.Maintenance;
using MaritimeEdge.Services.Reporting;
using MaritimeEdge.Services.Voyage;
using MaritimeEdge.Services.Logbooks;
using MaritimeEdge.Services.AbstractLog;
using MaritimeEdge.Services;
using MaritimeEdge.Repositories;

namespace MaritimeEdge
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configure default port - Listen on all network interfaces for mobile access
            // Can be overridden by command line: dotnet run --urls "http://0.0.0.0:5001"
            if (!args.Any(arg => arg.StartsWith("--urls")))
            {
                builder.WebHost.UseUrls("http://0.0.0.0:5001");
            }

            // Add services to the container
            var connectionString = builder.Configuration.GetValue<string>("Database:ConnectionString");
            
            // HTTP context accessor for audit interceptor
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<AuditInterceptor>();

            builder.Services.AddDbContext<EdgeDbContext>((sp, options) =>
                options.UseNpgsql(connectionString)
                       .AddInterceptors(sp.GetRequiredService<AuditInterceptor>())
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
            builder.Services.AddScoped<IReportingService, ReportingService>();
            builder.Services.AddScoped<IAggregateReportService, AggregateReportService>();
            builder.Services.AddScoped<ISyncService, SyncService>();
            builder.Services.AddScoped<IWatchkeepingService, WatchkeepingService>();
            builder.Services.AddScoped<IDeckLogbookService, DeckLogbookService>();
            builder.Services.AddScoped<IEngineLogbookService, EngineLogbookService>();
            builder.Services.AddScoped<IGarbageRecordService, GarbageRecordService>();
            builder.Services.AddScoped<IGarbagePartIService, GarbagePartIService>();
            builder.Services.AddScoped<IGarbagePartIIService, GarbagePartIIService>();
            builder.Services.AddScoped<IBallastWaterService, BallastWaterService>();
            builder.Services.AddScoped<IOilRecordService, OilRecordService>();
            builder.Services.AddScoped<IVoyageLogService, VoyageLogService>();
            builder.Services.AddScoped<MaterialReceiptService>();

            // Add Authentication & System Log Services (ISPS/ISM/IMO MSC.428)
            builder.Services.AddScoped<ISystemLogService, SystemLogService>();
            builder.Services.AddScoped<IAuthService, AuthService>();

            // Add Voyage Management Service
            builder.Services.AddScoped<IVoyageManagementService, VoyageManagementService>();

            // Add Abstract Log Service
            builder.Services.AddScoped<IAbstractLogService, AbstractLogService>();

            // Add PMS Repositories
            builder.Services.AddScoped<IEquipmentAssetRepository, EquipmentAssetRepository>();
            builder.Services.AddScoped<IMaintenanceScheduleRepository, MaintenanceScheduleRepository>();

            // Add PMS Services
            builder.Services.AddScoped<MaintenanceCompletionService>();

            // Add Ship Data Services
            builder.Services.AddScoped<IShipDataRepository, ShipDataRepository>();
            builder.Services.AddScoped<IShipDataService, ShipDataService>();

            // Add Background Services
            builder.Services.AddHostedService<MaritimeEdge.Services.Maintenance.MaintenanceSchedulerService>();
            builder.Services.AddHostedService<MaritimeEdge.Services.Voyage.TelemetrySimulatorService>();
            builder.Services.AddHostedService<MaritimeEdge.Services.Voyage.SignalKDataCollectorService>();
            builder.Services.AddHostedService<MaritimeEdge.Services.Core.DataCleanupService>();
            builder.Services.AddHostedService<MaritimeEdge.Services.Core.SyncBackgroundWorker>();

            // Add Controllers
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
                    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true; // Accept both camelCase and PascalCase input
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

            // Add Rate Limiting
            builder.Services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = 429;
                
                // Global fixed window: 100 requests per minute per IP
                options.AddPolicy("fixed", httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 100,
                            Window = TimeSpan.FromMinutes(1),
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 5
                        }));
                
                // Strict limiter for auth endpoints: 10 requests per minute per IP
                options.AddPolicy("auth", httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 10,
                            Window = TimeSpan.FromMinutes(1),
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 0
                        }));
            });

            var app = builder.Build();

            // Initialize database with migrations
            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<EdgeDbContext>();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                
                try
                {
                    logger.LogInformation("Checking database migrations...");
                    
                    var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();
                    if (pendingMigrations.Any())
                    {
                        logger.LogInformation($"Applying {pendingMigrations.Count()} pending migration(s)...");
                        await dbContext.Database.MigrateAsync();
                        logger.LogInformation("Database migrations applied successfully");
                    }
                    else
                    {
                        logger.LogInformation("Database is up-to-date, no pending migrations");
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error applying database migrations");
                    throw;
                }
            }

            // Configure the HTTP request pipeline
            
            // Global exception handler — MUST be first in pipeline
            app.UseMiddleware<GlobalExceptionMiddleware>();

            // Swagger only in Development
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Maritime Edge API v1");
                    c.RoutePrefix = "swagger";
                });
            }

            var uploadsPath = Path.Combine(app.Environment.ContentRootPath, "uploads");
            Directory.CreateDirectory(uploadsPath);
            
            // Create crew document subdirectories
            Directory.CreateDirectory(Path.Combine(uploadsPath, "crew", "documents", "travel_documents"));
            Directory.CreateDirectory(Path.Combine(uploadsPath, "crew", "documents", "seafarer_documents"));
            Directory.CreateDirectory(Path.Combine(uploadsPath, "crew", "documents", "employment_documents"));
            Directory.CreateDirectory(Path.Combine(uploadsPath, "crew", "documents", "health_documents"));
            
            // Enable CORS for ALL requests
            app.UseCors("AllowFrontend");
            
            // Configure static files with CORS headers explicitly
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(uploadsPath),
                RequestPath = "/uploads",
                OnPrepareResponse = ctx =>
                {
                    // Add CORS headers to static file responses
                    var origin = ctx.Context.Request.Headers["Origin"].ToString();
                    if (!string.IsNullOrEmpty(origin) && 
                        (origin.StartsWith("http://localhost:") || 
                         origin.StartsWith("http://192.168.") ||
                         origin.StartsWith("http://172.")))
                    {
                        ctx.Context.Response.Headers.Add("Access-Control-Allow-Origin", origin);
                        ctx.Context.Response.Headers.Add("Access-Control-Allow-Credentials", "true");
                    }
                }
            });

            app.UseRouting();

            // Rate limiting
            app.UseRateLimiter();

            // Session-based auth middleware - resolves Bearer token → user identity
            // MUST be before UseAuthorization so HttpContext.Items are populated
            // for AuditInterceptor and controller authorization checks
            app.UseMiddleware<SessionAuthMiddleware>();

            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}