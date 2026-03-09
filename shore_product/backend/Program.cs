using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json;
using ProductApi.Data;
using ProductApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Configuration
var configuration = builder.Configuration;

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DbContext
var conn = configuration.GetConnectionString("DefaultConnection") ?? "Host=postgres;Port=5432;Database=productdb;Username=product;Password=productpwd";
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(conn));

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWebMobile", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

// JWT
var jwtKey = configuration["JWT:Key"] ?? configuration["JWT__Key"] ?? "VerySecretKey12345";
var key = Encoding.ASCII.GetBytes(jwtKey);
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false
    };
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

// Register sync services (Phase 3)
builder.Services.AddScoped<ProductApi.Services.Sync.ISyncInboxService, ProductApi.Services.Sync.SyncInboxService>();
builder.Services.AddScoped<ProductApi.Services.Sync.ISyncOutboxService, ProductApi.Services.Sync.SyncOutboxService>();
builder.Services.AddScoped<ProductApi.Services.Sync.IConflictResolverService, ProductApi.Services.Sync.ConflictResolverService>();
builder.Services.AddScoped<ProductApi.Services.Sync.ICrewSyncOrchestrator, ProductApi.Services.Sync.CrewSyncOrchestrator>();

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
            logger.LogInformation($"Attempting database creation (Attempt {retryCount + 1}/5)...");
            db.Database.EnsureCreated();
            logger.LogInformation("Database creation/verification completed successfully.");
            break;
        }
        catch (Exception ex)
        {
            retryCount++;
            logger.LogError(ex, $"Database creation attempt {retryCount} failed.");
            if (retryCount >= 5) throw;
            await Task.Delay(5000); // Wait 5 seconds before retry
        }
    }
}

app.UseSwagger();
app.UseSwaggerUI();

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

app.UseCors("AllowWebMobile");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
