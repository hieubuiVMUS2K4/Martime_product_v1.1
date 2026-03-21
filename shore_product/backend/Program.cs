using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json;
using System.Threading.RateLimiting;
using ProductApi.Data;
using ProductApi.Security;
using ProductApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Configuration
var configuration = builder.Configuration;
var environment = builder.Environment;
var enforceHttps = configuration.GetValue("Security:EnforceHttps", false);

// Add services
builder.Services.AddControllers()
    .AddJsonOptions(opts =>
    {
        opts.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        opts.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        opts.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMemoryCache();

// DbContext
var conn = configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(conn))
    throw new InvalidOperationException("ConnectionStrings:DefaultConnection must be configured.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(conn)
           .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWebMobile", policy =>
    {
        var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
        if (allowedOrigins.Length > 0)
            policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod();
        else
            policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor |
                               ForwardedHeaders.XForwardedProto |
                               ForwardedHeaders.XForwardedHost;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// JWT
var jwtKey = configuration["JWT:Key"] ?? configuration["JWT__Key"];
if (string.IsNullOrWhiteSpace(jwtKey))
    throw new InvalidOperationException("JWT:Key must be configured via appsettings or environment variables.");

var key = Encoding.ASCII.GetBytes(jwtKey);
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = enforceHttps && !environment.IsDevelopment();
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

// Authorization policies — role-based access for Crew Management
builder.Services.AddAuthorizationBuilder()
    // HR Admin + Crew Coordinator — full crew management
    .AddPolicy("CrewManagement", policy =>
        policy.RequireRole("Admin", "HRAdmin", "CrewCoordinator", "SystemAdmin"))
    // Compliance Officer — compliance rules, waivers, document verification
    .AddPolicy("ComplianceManagement", policy =>
        policy.RequireRole("Admin", "ComplianceOfficer", "SystemAdmin"))
    // Travel Coordinator — travel requests and itineraries
    .AddPolicy("TravelManagement", policy =>
        policy.RequireRole("Admin", "TravelCoordinator", "CrewCoordinator", "SystemAdmin"))
    // Fleet Manager + Port Captain — planning, assignments, external requests
    .AddPolicy("FleetManagement", policy =>
        policy.RequireRole("Admin", "FleetManager", "PortCaptain", "CrewCoordinator", "SystemAdmin"))
    // Master (edge) — onboard events, sign-on/sign-off
    .AddPolicy("OnboardManagement", policy =>
        policy.RequireRole("Admin", "Master", "ChiefOfficer", "CrewCoordinator", "SystemAdmin"))
    // Read-only access for authenticated users
    .AddPolicy("CrewReadOnly", policy =>
        policy.RequireAuthenticatedUser())
    // Internal operational access for observability and admin sync endpoints.
    .AddPolicy("InternalAccess", policy =>
        policy.Requirements.Add(new InternalAccessRequirement()));

builder.Services.AddSingleton<IAuthorizationHandler, InternalAccessHandler>();
builder.Services.AddScoped<ProductApi.Security.SyncRequestVerificationMiddleware>();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("sync", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 240,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 10,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst
            }));

    options.AddPolicy("observability", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 30,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst
            }));

    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 600,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 20,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst
            }));
});

// Add Redis cache (commented out until package is available)
// builder.Services.AddStackExchangeRedisCache(options =>
// {
//     var redisConn = configuration.GetConnectionString("Redis") ?? "redis:6379";
//     options.Configuration = redisConn;
//     options.InstanceName = "MaritimeManagement";
// });

// DI for services/repositories
builder.Services.AddScoped<IShipService, ShipService>();
builder.Services.AddScoped<IShipRepository, ShipRepository>();

// Register maritime services
builder.Services.AddScoped<IVesselService, VesselService>();
builder.Services.AddScoped<IAlertService, AlertService>();
builder.Services.AddScoped<ITelemetryService, TelemetryService>();

// Register crew management services (Phase 2)
builder.Services.AddScoped<ProductApi.Services.Crew.ICrewService, ProductApi.Services.Crew.CrewService>();
builder.Services.AddScoped<ProductApi.Services.Crew.ICertificateService, ProductApi.Services.Crew.CertificateService>();

// Register crew management workflow services (Phase 1A)
builder.Services.AddScoped<ProductApi.Services.CrewManagement.IAuditService, ProductApi.Services.CrewManagement.AuditService>();
builder.Services.AddScoped<ProductApi.Services.CrewManagement.ICrewStatusService, ProductApi.Services.CrewManagement.CrewStatusService>();
builder.Services.AddScoped<ProductApi.Services.CrewManagement.IOnboardingService, ProductApi.Services.CrewManagement.OnboardingService>();
builder.Services.AddScoped<ProductApi.Services.CrewManagement.IDocumentWorkflowService, ProductApi.Services.CrewManagement.DocumentWorkflowService>();

// Register compliance service (Phase 2)
builder.Services.AddScoped<ProductApi.Services.CrewManagement.IComplianceService, ProductApi.Services.CrewManagement.ComplianceService>();

// Register assignment service (Phase 5)
builder.Services.AddScoped<ProductApi.Services.CrewManagement.IAssignmentService, ProductApi.Services.CrewManagement.AssignmentService>();

// Register external request & travel services (Phase 6)
builder.Services.AddScoped<ProductApi.Services.CrewManagement.IExternalRequestService, ProductApi.Services.CrewManagement.ExternalRequestService>();
builder.Services.AddScoped<ProductApi.Services.CrewManagement.ITravelService, ProductApi.Services.CrewManagement.TravelService>();

// Register onboard event service (Phase 7)
builder.Services.AddScoped<ProductApi.Services.CrewManagement.IOnboardEventService, ProductApi.Services.CrewManagement.OnboardEventService>();

// Register sync services (Phase 3)
builder.Services.AddScoped<ProductApi.Services.Sync.ISyncInboxService, ProductApi.Services.Sync.SyncInboxService>();
builder.Services.AddScoped<ProductApi.Services.Sync.ISyncOutboxService, ProductApi.Services.Sync.SyncOutboxService>();
builder.Services.AddScoped<ProductApi.Services.Sync.IConflictResolverService, ProductApi.Services.Sync.ConflictResolverService>();
builder.Services.AddScoped<ProductApi.Services.INotificationService, ProductApi.Services.NotificationService>();
builder.Services.AddScoped<ProductApi.Services.Sync.ICrewSyncOrchestrator, ProductApi.Services.Sync.CrewSyncOrchestrator>();

// Register voyage management service
builder.Services.AddScoped<ProductApi.Services.Voyage.IVoyageService, ProductApi.Services.Voyage.VoyageService>();

// Background services
builder.Services.AddHostedService<AlertBackgroundService>();
builder.Services.AddHostedService<ProductApi.Services.Sync.CertificateExpiryMonitorService>();
builder.Services.AddHostedService<ProductApi.Services.Sync.SyncHealthMonitorService>();

var app = builder.Build();

// Migrate DB using EF Core Migrations
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var retryCount = 0;
    while (retryCount < 5)
    {
        try
        {
            logger.LogInformation($"Attempting database migration (Attempt {retryCount + 1}/5)...");
            db.Database.Migrate();
            await db.Database.ExecuteSqlRawAsync(@"
                CREATE TABLE IF NOT EXISTS voyage_reviews (
                    ""Id"" uuid NOT NULL,
                    ""VoyageId"" uuid NOT NULL,
                    ""ReviewStatus"" character varying(30) NOT NULL,
                    ""Notes"" text,
                    ""ReviewedBy"" character varying(100),
                    ""ReviewedAt"" timestamp with time zone,
                    ""Tags"" text,
                    ""CreatedAt"" timestamp with time zone NOT NULL,
                    ""UpdatedAt"" timestamp with time zone NOT NULL,
                    CONSTRAINT ""PK_voyage_reviews"" PRIMARY KEY (""Id""),
                    CONSTRAINT ""FK_voyage_reviews_voyage_records_VoyageId"" FOREIGN KEY (""VoyageId"") REFERENCES voyage_records (""Id"") ON DELETE CASCADE
                );

                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_voyage_reviews_VoyageId"" ON voyage_reviews (""VoyageId"");
                CREATE INDEX IF NOT EXISTS ""IX_voyage_reviews_ReviewStatus"" ON voyage_reviews (""ReviewStatus"");
            ");
            await db.Database.ExecuteSqlRawAsync(@"
                ALTER TABLE crew_members
                ADD COLUMN IF NOT EXISTS ""VesselId"" uuid;

                CREATE INDEX IF NOT EXISTS ""IX_crew_members_VesselId"" ON crew_members (""VesselId"");

                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1
                        FROM information_schema.tables
                        WHERE table_schema = 'public' AND table_name = 'crew_assignments'
                    ) THEN
                        UPDATE crew_members AS cm
                        SET ""VesselId"" = src.""VesselId""
                        FROM (
                            SELECT DISTINCT ON (ca.""CrewMemberId"")
                                ca.""CrewMemberId"",
                                ca.""VesselId""
                            FROM crew_assignments AS ca
                            WHERE ca.""Status"" IN (
                                'OnBoarded',
                                'ReadyToJoin',
                                'TravelInProgress',
                                'Confirmed',
                                'PendingCrewConfirmation',
                                'Proposed',
                                'Draft'
                            )
                            ORDER BY
                                ca.""CrewMemberId"",
                                CASE ca.""Status""
                                    WHEN 'OnBoarded' THEN 1
                                    WHEN 'ReadyToJoin' THEN 2
                                    WHEN 'TravelInProgress' THEN 3
                                    WHEN 'Confirmed' THEN 4
                                    WHEN 'PendingCrewConfirmation' THEN 5
                                    WHEN 'Proposed' THEN 6
                                    WHEN 'Draft' THEN 7
                                    ELSE 99
                                END,
                                COALESCE(ca.""ActualStartDate"", ca.""PlannedStartDate"", ca.""UpdatedAt"", ca.""CreatedAt"") DESC
                        ) AS src
                        WHERE cm.""Id"" = src.""CrewMemberId""
                          AND cm.""VesselId"" IS NULL;
                    END IF;
                END $$;
            ");
            // ── PMS: add VesselId to equipment_assets & material_items ──
            await db.Database.ExecuteSqlRawAsync(@"
                ALTER TABLE equipment_assets
                ADD COLUMN IF NOT EXISTS ""VesselId"" uuid;

                DROP INDEX IF EXISTS ""IX_equipment_assets_AssetCode"";
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_equipment_assets_VesselId_AssetCode""
                    ON equipment_assets (""VesselId"", ""AssetCode"");

                ALTER TABLE material_items
                ADD COLUMN IF NOT EXISTS ""VesselId"" uuid;

                DROP INDEX IF EXISTS ""IX_material_items_ItemCode"";
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_material_items_VesselId_ItemCode""
                    ON material_items (""VesselId"", ""ItemCode"");

                ALTER TABLE ""MaintenanceTasks""
                ADD COLUMN IF NOT EXISTS ""VesselId"" uuid;

                CREATE INDEX IF NOT EXISTS ""IX_MaintenanceTasks_VesselId""
                    ON ""MaintenanceTasks"" (""VesselId"");

                -- Backfill VesselId from OriginNode (IMO) for existing tasks
                UPDATE ""MaintenanceTasks"" AS mt
                SET ""VesselId"" = v.""Id""
                FROM ""Vessels"" AS v
                WHERE mt.""VesselId"" IS NULL
                  AND mt.""OriginNode"" IS NOT NULL
                  AND mt.""OriginNode"" <> ''
                  AND mt.""OriginNode"" <> 'SHORE'
                  AND v.""IMO"" = mt.""OriginNode"";
            ");

            // ── Shore notifications table ──
            await db.Database.ExecuteSqlRawAsync(@"
                CREATE TABLE IF NOT EXISTS shore_notifications (
                    ""Id""         uuid PRIMARY KEY DEFAULT gen_random_uuid(),
                    ""Type""       character varying(50)  NOT NULL,
                    ""Title""      character varying(200) NOT NULL,
                    ""Message""    character varying(500) NOT NULL,
                    ""VesselId""   uuid,
                    ""VesselName"" character varying(200),
                    ""CrewMemberId"" uuid,
                    ""CrewName""   character varying(200),
                    ""CreatedAt""  timestamp with time zone NOT NULL DEFAULT NOW(),
                    ""IsRead""     boolean NOT NULL DEFAULT false
                );

                CREATE INDEX IF NOT EXISTS ""IX_shore_notifications_CreatedAt""
                    ON shore_notifications (""CreatedAt"");
                CREATE INDEX IF NOT EXISTS ""IX_shore_notifications_IsRead""
                    ON shore_notifications (""IsRead"");
            ");

            logger.LogInformation("Database migration/verification completed successfully.");
            break;
        }
        catch (Exception ex)
        {
            retryCount++;
            logger.LogError(ex, $"Database migration attempt {retryCount} failed.");
            if (retryCount >= 5) throw;
            await Task.Delay(5000); // Wait 5 seconds before retry
        }
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseForwardedHeaders();

if (!app.Environment.IsDevelopment() && enforceHttps)
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

// Global exception handling middleware
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.ContentType = "application/json";
        var exceptionFeature = context.Features.Get<IExceptionHandlerFeature>();
        var exception = exceptionFeature?.Error;

        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(exception, "Unhandled exception on {Method} {Path}", 
            context.Request.Method, context.Request.Path);

        var (statusCode, message) = exception switch
        {
            ArgumentNullException => (StatusCodes.Status400BadRequest, "Missing required parameter"),
            ArgumentException => (StatusCodes.Status400BadRequest, "Invalid parameter"),
            KeyNotFoundException => (StatusCodes.Status404NotFound, "Resource not found"),
            UnauthorizedAccessException => (StatusCodes.Status403Forbidden, "Access denied"),
            InvalidOperationException => (StatusCodes.Status409Conflict, "Operation conflict"),
            OperationCanceledException => (StatusCodes.Status408RequestTimeout, "Request timed out"),
            DbUpdateException => (StatusCodes.Status409Conflict, "Database constraint violation"),
            _ => (StatusCodes.Status500InternalServerError, "An internal error occurred")
        };

        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsync(JsonSerializer.Serialize(new
        {
            error = message,
            status = statusCode,
            traceId = context.TraceIdentifier
        }));
    });
});

// Create uploads directories
var uploadsPath = Path.Combine(app.Environment.ContentRootPath, "uploads");
Directory.CreateDirectory(uploadsPath);
Directory.CreateDirectory(Path.Combine(uploadsPath, "crew", "certificates"));
Directory.CreateDirectory(Path.Combine(uploadsPath, "crew", "avatars"));
Directory.CreateDirectory(Path.Combine(uploadsPath, "crew", "documents", "travel_documents"));
Directory.CreateDirectory(Path.Combine(uploadsPath, "crew", "documents", "seafarer_documents"));
Directory.CreateDirectory(Path.Combine(uploadsPath, "crew", "documents", "employment_documents"));
Directory.CreateDirectory(Path.Combine(uploadsPath, "crew", "documents", "health_documents"));

app.UseCors("AllowWebMobile");
app.UseRouting();
app.UseRateLimiter();
app.UseWhen(
    context => ProductApi.Security.SyncRequestVerificationMiddleware.IsProtectedSyncRequest(context.Request),
    branch => branch.UseMiddleware<ProductApi.Security.SyncRequestVerificationMiddleware>());

// Serve uploaded files
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads"
});

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
