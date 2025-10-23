using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
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

// Add background service for automatic alerts
builder.Services.AddHostedService<AlertBackgroundService>();

var app = builder.Build();

// Migrate DB (ensure created) with retry logic
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var retryCount = 0;
    while (retryCount < 5)
    {
        try
        {
            db.Database.EnsureCreated();
            break;
        }
        catch (Exception ex)
        {
            retryCount++;
            Console.WriteLine($"Database connection attempt {retryCount} failed: {ex.Message}");
            if (retryCount >= 5) throw;
            await Task.Delay(5000); // Wait 5 seconds before retry
        }
    }
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowWebMobile");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
