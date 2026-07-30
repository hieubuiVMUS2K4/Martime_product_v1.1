using MaritimeEdge.Data;
using MaritimeEdge.Security;
using Microsoft.EntityFrameworkCore;

namespace MaritimeEdge.Services.Core;

/// <summary>
/// Result of <see cref="IEdgeRuntimeConfigService.GetSyncConfigAsync"/>. Contains the resolved
/// sync/identity configuration for this Edge node, regardless of source (DB profile vs legacy env).
/// </summary>
public class EdgeSyncConfig
{
    public string NodeId { get; set; } = string.Empty;
    public string ShoreBaseUrl { get; set; } = string.Empty;
    public string NodeApiToken { get; set; } = string.Empty;
    public string SigningKey { get; set; } = string.Empty;
    public int KeyVersion { get; set; } = 1;
    public string ProtocolVersion { get; set; } = "2";
    public bool SecurityEnabled { get; set; }
    public Guid? ShoreVesselId { get; set; }
    public string? VesselImo { get; set; }
    public string? VesselName { get; set; }
    public int BatchSize { get; set; } = 100;
    public int SyncIntervalSec { get; set; } = 30;
    public string NetworkType { get; set; } = "Shore_WiFi";
    public string Source { get; set; } = "db"; // "db" | "legacy_config"
}

/// <summary>
/// Thrown when Managed Mode is required but no active <see cref="MaritimeEdge.Models.EdgeProvisioningProfile"/>
/// exists yet. Callers MUST treat this as "stop sync" (Fail-Closed) — never silently fall back to
/// hardcoded defaults.
/// </summary>
public class ProvisioningRequiredException : Exception
{
    public ProvisioningRequiredException(string message) : base(message) { }
}

/// <summary>
/// Thrown when the active profile exists but is unusable (decrypt failure, missing required field).
/// Callers MUST treat this as "stop sync" (Fail-Closed).
/// </summary>
public class ConfigInvalidException : Exception
{
    public ConfigInvalidException(string message) : base(message) { }
}

public interface IEdgeRuntimeConfigService
{
    /// <summary>
    /// Resolves the active sync configuration. Managed Mode (DB-backed) takes priority whenever an
    /// active profile row exists. Falls back to legacy <c>IConfiguration</c> values (SyncSecurity:*,
    /// ShoreAPI:*) ONLY when no active profile exists AND config mode is explicitly Legacy
    /// (<c>EDGE_SYNC_CONFIG_MODE=Legacy</c>). Otherwise throws <see cref="ProvisioningRequiredException"/>
    /// or <see cref="ConfigInvalidException"/> — Fail-Closed, no silent fallback.
    /// </summary>
    Task<EdgeSyncConfig> GetSyncConfigAsync();

    Task<bool> HasActiveProfileAsync();
}

public class EdgeRuntimeConfigService : IEdgeRuntimeConfigService
{
    private readonly EdgeDbContext _db;
    private readonly IEdgeDataEncryptionService _encryption;
    private readonly IConfiguration _configuration;
    private readonly ILogger<EdgeRuntimeConfigService> _logger;

    public EdgeRuntimeConfigService(
        EdgeDbContext db,
        IEdgeDataEncryptionService encryption,
        IConfiguration configuration,
        ILogger<EdgeRuntimeConfigService> logger)
    {
        _db = db;
        _encryption = encryption;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<bool> HasActiveProfileAsync()
    {
        return await _db.EdgeProvisioningProfiles.AsNoTracking().AnyAsync(p => p.IsActive);
    }

    public async Task<EdgeSyncConfig> GetSyncConfigAsync()
    {
        var activeProfile = await _db.EdgeProvisioningProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.IsActive);

        if (activeProfile != null)
        {
            // A profile exists, so Managed Mode owns sync config. If it is corrupted, fail closed.
            return BuildFromProfile(activeProfile);
        }

        var configMode = _configuration["Sync:ConfigMode"]
                          ?? Environment.GetEnvironmentVariable("EDGE_SYNC_CONFIG_MODE");

        if (string.Equals(configMode, "Legacy", StringComparison.OrdinalIgnoreCase))
        {
            // Explicit legacy opt-in for old deployments only. Fresh-start provisioning must not
            // sync with old appsettings/.env identity before import/activate.
            return BuildFromLegacyConfig();
        }

        throw new ProvisioningRequiredException(
            "No active Vessel Provisioning Profile. Sync is paused until a provisioning package " +
            "has been imported and activated. Set EDGE_SYNC_CONFIG_MODE=Legacy only for old " +
            "deployments that explicitly need legacy fallback.");
    }
    private EdgeSyncConfig BuildFromProfile(Models.EdgeProvisioningProfile profile)
    {
        if (string.IsNullOrWhiteSpace(profile.NodeId) ||
            string.IsNullOrWhiteSpace(profile.ShoreBaseUrl) ||
            string.IsNullOrWhiteSpace(profile.NodeApiToken))
        {
            throw new ConfigInvalidException(
                $"EdgeProvisioningProfile #{profile.Id} thiếu trường bắt buộc (NodeId/ShoreBaseUrl/NodeApiToken).");
        }

        string nodeApiToken;
        string signingKey;
        try
        {
            nodeApiToken = _encryption.Decrypt(profile.NodeApiToken) ?? string.Empty;
            signingKey = _encryption.Decrypt(profile.SigningKey) ?? string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to decrypt EdgeProvisioningProfile #{ProfileId} credentials.", profile.Id);
            throw new ConfigInvalidException(
                $"Không thể giải mã credentials của EdgeProvisioningProfile #{profile.Id}. " +
                "Có thể EDGE_DATA_PROTECTION_KEY đã thay đổi — cần import lại provisioning package.");
        }

        if (string.IsNullOrEmpty(nodeApiToken))
        {
            throw new ConfigInvalidException(
                $"EdgeProvisioningProfile #{profile.Id} không có NodeApiToken hợp lệ sau khi giải mã.");
        }

        return new EdgeSyncConfig
        {
            NodeId = profile.NodeId!,
            ShoreBaseUrl = profile.ShoreBaseUrl!.TrimEnd('/'),
            NodeApiToken = nodeApiToken,
            SigningKey = signingKey,
            KeyVersion = profile.KeyVersion,
            ProtocolVersion = profile.ProtocolVersion,
            SecurityEnabled = profile.SecurityEnabled,
            ShoreVesselId = profile.VesselId,
            VesselImo = profile.VesselImo,
            VesselName = profile.VesselName,
            BatchSize = profile.BatchSize > 0 ? profile.BatchSize : 100,
            SyncIntervalSec = profile.SyncIntervalSec > 0 ? profile.SyncIntervalSec : 30,
            NetworkType = string.IsNullOrWhiteSpace(profile.NetworkType) ? "Shore_WiFi" : profile.NetworkType,
            Source = "db"
        };
    }

    private EdgeSyncConfig BuildFromLegacyConfig()
    {
        var nodeId = _configuration["SyncSecurity:NodeId"];
        var shoreBaseUrl = _configuration["ShoreAPI:BaseUrl"];
        var nodeApiToken = _configuration["NodeApiToken"] ?? _configuration["ShoreAPI:ApiKey"];
        var signingKey = _configuration["SyncSecurity:SigningKey"];

        if (string.IsNullOrWhiteSpace(nodeId) || string.IsNullOrWhiteSpace(shoreBaseUrl) ||
            string.IsNullOrWhiteSpace(nodeApiToken))
        {
            throw new ConfigInvalidException(
                "Legacy Mode được yêu cầu nhưng thiếu SyncSecurity:NodeId / ShoreAPI:BaseUrl / NodeApiToken " +
                "trong cấu hình (.env). Không có cấu hình hợp lệ để chạy sync — Fail Closed.");
        }

        Guid? shoreVesselId = null;
        if (Guid.TryParse(_configuration["ShoreAPI:VesselId"], out var parsed))
            shoreVesselId = parsed;

        return new EdgeSyncConfig
        {
            NodeId = nodeId,
            ShoreBaseUrl = shoreBaseUrl.TrimEnd('/'),
            NodeApiToken = nodeApiToken,
            SigningKey = signingKey ?? string.Empty,
            KeyVersion = int.TryParse(_configuration["SyncSecurity:KeyVersion"], out var kv) ? kv : 1,
            ProtocolVersion = _configuration["SyncSecurity:ProtocolVersion"] ?? "2",
            SecurityEnabled = bool.TryParse(_configuration["SyncSecurity:Enabled"], out var enabled) && enabled,
            ShoreVesselId = shoreVesselId,
            VesselImo = _configuration["Vessel:IMO"],
            VesselName = _configuration["Vessel:Name"],
            BatchSize = _configuration.GetValue("Sync:BatchSize", 100),
            SyncIntervalSec = _configuration.GetValue("Sync:SyncIntervalSec", _configuration.GetValue("Sync:HighPriorityInterval", 30)),
            NetworkType = _configuration["Sync:NetworkType"] ?? "Shore_WiFi",
            Source = "legacy_config"
        };
    }
}
