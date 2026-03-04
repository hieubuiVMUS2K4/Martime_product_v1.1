using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using MaritimeEdge.Models;
using MaritimeEdge.Models.Inventory;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MaritimeEdge.Services.Core;

/// <summary>
/// EF Core SaveChanges Interceptor - Automatic Audit Trail
/// 
/// Compliance:
/// - ISM Code Chapter 12: Company Verification, Review, and Audit
/// - IMO MSC.428(98): Maritime Cyber Risk Management
/// - ISO 27001: A.12.4 Logging and Monitoring
/// - GDPR Art.30: Records of Processing Activities
/// 
/// Design:
/// - Intercepts all INSERT/UPDATE/DELETE operations
/// - Captures old + new values as JSON for full audit trail
/// - Uses IHttpContextAccessor to resolve current user (zero coupling to controllers)
/// - Excludes high-volume telemetry tables for performance
/// - Fire-and-forget: audit failures never break business operations
/// </summary>
public class AuditInterceptor : SaveChangesInterceptor
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AuditInterceptor> _logger;

    // High-volume tables excluded from audit (telemetry/sensor data)
    private static readonly HashSet<string> ExcludedEntities = new(StringComparer.OrdinalIgnoreCase)
    {
        nameof(NmeaRawData),
        nameof(PositionData),
        nameof(AisData),
        nameof(NavigationData),
        nameof(EngineData),
        nameof(GeneratorData),
        nameof(TankLevel),
        nameof(FuelConsumption),
        nameof(EnvironmentalData),
        nameof(SystemLog),         // Don't audit the audit log itself
        nameof(LoginAttempt),      // Already tracked separately
        nameof(SyncQueue),         // Internal sync mechanism
        nameof(UserSession),       // Session updates are too frequent
        nameof(FuelAnalyticsSummary),
        nameof(FuelEfficiencyAlert),
    };

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        MaxDepth = 3,
    };

    public AuditInterceptor(
        IHttpContextAccessor httpContextAccessor,
        ILogger<AuditInterceptor> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is null) return base.SavingChangesAsync(eventData, result, cancellationToken);

        var entries = eventData.Context.ChangeTracker
            .Entries()
            .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .Where(e => !ExcludedEntities.Contains(e.Entity.GetType().Name))
            .ToList();

        if (entries.Count == 0) return base.SavingChangesAsync(eventData, result, cancellationToken);

        var (userId, username) = ResolveCurrentUser();
        var ipAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
        var userAgent = _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].FirstOrDefault();

        foreach (var entry in entries)
        {
            try
            {
                var entityName = entry.Entity.GetType().Name;
                var entityId = GetPrimaryKeyValue(entry);
                var action = entry.State switch
                {
                    EntityState.Added => "RECORD_CREATED",
                    EntityState.Modified => "RECORD_UPDATED",
                    EntityState.Deleted => "RECORD_DELETED",
                    _ => "UNKNOWN"
                };

                string? oldValues = null;
                string? newValues = null;

                if (entry.State == EntityState.Modified)
                {
                    var changed = new Dictionary<string, object?>();
                    var original = new Dictionary<string, object?>();

                    foreach (var prop in entry.Properties.Where(p => p.IsModified))
                    {
                        original[prop.Metadata.Name] = prop.OriginalValue;
                        changed[prop.Metadata.Name] = prop.CurrentValue;
                    }

                    if (changed.Count > 0)
                    {
                        oldValues = SafeSerialize(original);
                        newValues = SafeSerialize(changed);
                    }
                    else
                    {
                        continue; // No actual changes
                    }
                }
                else if (entry.State == EntityState.Added)
                {
                    var values = entry.Properties
                        .Where(p => p.CurrentValue != null)
                        .ToDictionary(p => p.Metadata.Name, p => p.CurrentValue);
                    newValues = SafeSerialize(values);
                }
                else if (entry.State == EntityState.Deleted)
                {
                    var values = entry.Properties
                        .Where(p => p.OriginalValue != null)
                        .ToDictionary(p => p.Metadata.Name, p => p.OriginalValue);
                    oldValues = SafeSerialize(values);
                }

                var log = new SystemLog
                {
                    Timestamp = DateTime.UtcNow,
                    Category = "DATA",
                    Action = action,
                    Level = entry.State == EntityState.Deleted ? "WARNING" : "INFO",
                    Message = $"{action}: {entityName} [{entityId}]",
                    UserId = userId,
                    Username = username,
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    EntityType = entityName,
                    EntityId = entityId,
                    OldValues = oldValues,
                    NewValues = newValues,
                    Result = "SUCCESS",
                    IsSynced = false
                };

                eventData.Context.Set<SystemLog>().Add(log);
            }
            catch (Exception ex)
            {
                // Audit must never break the main operation
                _logger.LogWarning(ex, "Failed to create audit entry for {Entity}", entry.Entity.GetType().Name);
            }
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private (long? userId, string? username) ResolveCurrentUser()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext is null) return (null, "SYSTEM");

        // Primary: Read from SessionAuthMiddleware (HttpContext.Items)
        var userId = httpContext.GetUserId();
        var username = httpContext.GetUsername();

        if (userId.HasValue)
            return (userId.Value, username ?? "UNKNOWN");

        // Fallback: Check X-User-Id header (for internal/background service calls)
        var userIdHeader = httpContext.Request.Headers["X-User-Id"].FirstOrDefault();
        var usernameHeader = httpContext.Request.Headers["X-Username"].FirstOrDefault();

        if (long.TryParse(userIdHeader, out var uid))
            return (uid, usernameHeader ?? "UNKNOWN");

        return (null, "ANONYMOUS");
    }

    private static string? GetPrimaryKeyValue(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry)
    {
        var keyProps = entry.Metadata.FindPrimaryKey()?.Properties;
        if (keyProps == null || keyProps.Count == 0) return null;

        // For deleted entities, CurrentValue may be null — always use OriginalValue for PK
        var values = keyProps.Select(p =>
        {
            var prop = entry.Property(p.Name);
            return (entry.State == EntityState.Deleted ? prop.OriginalValue : prop.CurrentValue)?.ToString();
        });
        return string.Join(",", values);
    }

    private static string? SafeSerialize(object? value)
    {
        if (value == null) return null;
        try
        {
            var json = JsonSerializer.Serialize(value, JsonOpts);
            // Truncate to 4000 chars to avoid DB column overflow
            return json.Length > 4000 ? json[..4000] + "..." : json;
        }
        catch
        {
            return "[serialization error]";
        }
    }
}
