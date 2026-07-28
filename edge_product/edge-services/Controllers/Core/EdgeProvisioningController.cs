using System.IO.Compression;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MaritimeEdge.Data;
using MaritimeEdge.Models;
using MaritimeEdge.Security;
using MaritimeEdge.Services.Core;

namespace MaritimeEdge.Controllers.Core;

/// <summary>
/// Vessel Provisioning v3 — Component 6. Lets IT on the vessel import a Provisioning Package
/// (ZIP/JSON) exported from Shore, preview/test it, and activate it as the Managed Mode config
/// source of truth (<see cref="EdgeProvisioningProfile"/>).
/// </summary>
[ApiController]
[Route("api/edge/provisioning")]
[Authorize(Policy = "InternalAccess")]
public class EdgeProvisioningController : ControllerBase
{
    private static readonly string[] SupportedSchemaVersions = { "1.0" };
    private static readonly string[] SupportedProtocolVersions = { "1", "2" };

    private readonly EdgeDbContext _context;
    private readonly IEdgeDataEncryptionService _encryption;
    private readonly IEdgeRuntimeConfigService _runtimeConfigService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<EdgeProvisioningController> _logger;

    public EdgeProvisioningController(
        EdgeDbContext context,
        IEdgeDataEncryptionService encryption,
        IEdgeRuntimeConfigService runtimeConfigService,
        IHttpClientFactory httpClientFactory,
        ILogger<EdgeProvisioningController> logger)
    {
        _context = context;
        _encryption = encryption;
        _runtimeConfigService = runtimeConfigService;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <summary>GET /api/edge/provisioning/status</summary>
    [HttpGet("status")]
    public async Task<IActionResult> GetStatus()
    {
        var active = await _context.EdgeProvisioningProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.IsActive);

        if (active == null)
        {
            return Ok(new
            {
                isActive = false,
                nodeId = (string?)null,
                shoreUrl = (string?)null,
                lastHandshake = (DateTime?)null,
                handshakeStatus = (string?)null,
                source = "none"
            });
        }

        return Ok(new
        {
            isActive = true,
            profileId = active.Id,
            nodeId = active.NodeId,
            shoreUrl = active.ShoreBaseUrl,
            lastHandshake = active.LastHandshakeAt,
            handshakeStatus = active.HandshakeStatus,
            source = "db"
        });
    }

    /// <summary>
    /// POST /api/edge/provisioning/import — multipart/form-data upload of a Provisioning Package
    /// (.zip containing edge-provisioning.json, or a bare .json file). Validates required fields,
    /// encrypts secrets, and stores as an inactive profile (is_active=false) for preview/testing.
    /// </summary>
    [HttpPost("import")]
    [RequestSizeLimit(5 * 1024 * 1024)]
    public async Task<IActionResult> Import(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { error = "Không có file được tải lên." });

        string jsonContent;
        try
        {
            jsonContent = await ExtractProvisioningJsonAsync(file);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = $"Không đọc được file provisioning: {ex.Message}" });
        }

        JsonDocument doc;
        try
        {
            doc = JsonDocument.Parse(jsonContent);
        }
        catch (JsonException ex)
        {
            return BadRequest(new { error = $"File edge-provisioning.json không hợp lệ (JSON lỗi): {ex.Message}" });
        }

        using (doc)
        {
            var root = doc.RootElement;

            var validationError = ValidateProvisioningJson(root, out var parsed);
            if (validationError != null)
                return BadRequest(new { error = validationError });

            var profile = new EdgeProvisioningProfile
            {
                IsActive = false,
                NodeId = parsed.NodeId,
                VesselImo = parsed.VesselImo,
                VesselName = parsed.VesselName,
                VesselId = parsed.VesselId,
                ShoreBaseUrl = parsed.ShoreBaseUrl,
                NodeApiToken = _encryption.Encrypt(parsed.NodeApiToken),
                SigningKey = _encryption.Encrypt(parsed.SigningKey),
                KeyVersion = parsed.KeyVersion,
                ProtocolVersion = parsed.ProtocolVersion,
                SecurityEnabled = parsed.SecurityEnabled,
                BatchSize = parsed.BatchSize,
                SyncIntervalSec = parsed.SyncIntervalSec,
                NetworkType = parsed.NetworkType,
                SchemaVersion = parsed.SchemaVersion,
                ImportedAt = DateTime.UtcNow,
                ImportedFrom = file.FileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) ? "zip_upload" : "json_upload",
                HandshakeStatus = "never"
            };

            _context.EdgeProvisioningProfiles.Add(profile);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Imported EdgeProvisioningProfile #{ProfileId} (NodeId={NodeId}, Source={Source})",
                profile.Id, profile.NodeId, profile.ImportedFrom);

            return Ok(new
            {
                profileId = profile.Id,
                preview = new
                {
                    nodeId = profile.NodeId,
                    vesselImo = profile.VesselImo,
                    vesselName = profile.VesselName,
                    shoreBaseUrl = profile.ShoreBaseUrl,
                    keyVersion = profile.KeyVersion,
                    protocolVersion = profile.ProtocolVersion,
                    securityEnabled = profile.SecurityEnabled
                }
            });
        }
    }

    public class ProfileActionRequest
    {
        public int ProfileId { get; set; }
    }

    /// <summary>
    /// POST /api/edge/provisioning/test — Uses a not-yet-activated profile to call Shore's
    /// POST /api/sync/handshake and confirm the credentials/URL actually work before activating.
    /// </summary>
    [HttpPost("test")]
    public async Task<IActionResult> TestConnection([FromBody] ProfileActionRequest request)
    {
        var profile = await _context.EdgeProvisioningProfiles.FirstOrDefaultAsync(p => p.Id == request.ProfileId);
        if (profile == null)
            return NotFound(new { error = $"Không tìm thấy profile #{request.ProfileId}." });

        if (string.IsNullOrWhiteSpace(profile.ShoreBaseUrl) || string.IsNullOrWhiteSpace(profile.NodeId))
            return BadRequest(new { error = "Profile thiếu ShoreBaseUrl hoặc NodeId." });

        string rawToken;
        try
        {
            rawToken = _encryption.Decrypt(profile.NodeApiToken) ?? string.Empty;
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = $"Không giải mã được NodeApiToken: {ex.Message}" });
        }

        try
        {
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(profile.ShoreBaseUrl.TrimEnd('/') + "/");
            client.Timeout = TimeSpan.FromSeconds(15);
            client.DefaultRequestHeaders.Add("X-Node-Api-Token", rawToken);

            var body = JsonSerializer.Serialize(new
            {
                nodeId = profile.NodeId,
                vesselImo = profile.VesselImo,
                edgeVersion = "3.0",
                networkType = profile.NetworkType ?? "Shore_WiFi"
            });

            var response = await client.PostAsync(
                "api/sync/handshake",
                new StringContent(body, System.Text.Encoding.UTF8, "application/json"));

            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                profile.HandshakeStatus = "failed";
                profile.LastHandshakeError = $"HTTP {(int)response.StatusCode}: {Truncate(responseBody, 500)}";
                profile.LastHandshakeAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return Ok(new { success = false, message = profile.LastHandshakeError });
            }

            profile.HandshakeStatus = "success";
            profile.LastHandshakeError = null;
            profile.LastHandshakeAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Kết nối thành công.", serverTime = DateTime.UtcNow });
        }
        catch (Exception ex)
        {
            profile.HandshakeStatus = "failed";
            profile.LastHandshakeError = Truncate(ex.Message, 500);
            profile.LastHandshakeAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogWarning(ex, "Test connection failed for profile #{ProfileId}", profile.Id);
            return Ok(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// POST /api/edge/provisioning/activate — Deactivates any currently active profile and
    /// activates the given one, atomically. This is the moment Managed Mode "goes live".
    /// </summary>
    [HttpPost("activate")]
    public async Task<IActionResult> Activate([FromBody] ProfileActionRequest request)
    {
        var profile = await _context.EdgeProvisioningProfiles.FirstOrDefaultAsync(p => p.Id == request.ProfileId);
        if (profile == null)
            return NotFound(new { error = $"Không tìm thấy profile #{request.ProfileId}." });

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var previouslyActive = await _context.EdgeProvisioningProfiles
                .Where(p => p.IsActive)
                .ToListAsync();

            var previousProfileId = previouslyActive.FirstOrDefault()?.Id;

            foreach (var p in previouslyActive)
                p.IsActive = false;

            profile.IsActive = true;
            profile.ActivatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation(
                "Activated EdgeProvisioningProfile #{ProfileId} (NodeId={NodeId}); previous active profile: #{PreviousProfileId}",
                profile.Id, profile.NodeId, previousProfileId);

            return Ok(new { activated = true, previousProfileId });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Failed to activate EdgeProvisioningProfile #{ProfileId}", request.ProfileId);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>GET /api/edge/provisioning/history</summary>
    [HttpGet("history")]
    public async Task<IActionResult> GetHistory()
    {
        var profiles = await _context.EdgeProvisioningProfiles
            .AsNoTracking()
            .OrderByDescending(p => p.ImportedAt)
            .Select(p => new
            {
                p.Id,
                p.IsActive,
                p.NodeId,
                p.VesselImo,
                p.VesselName,
                p.ShoreBaseUrl,
                p.KeyVersion,
                p.ImportedAt,
                p.ImportedFrom,
                p.ActivatedAt,
                p.LastHandshakeAt,
                p.HandshakeStatus
            })
            .ToListAsync();

        return Ok(profiles);
    }

    private static async Task<string> ExtractProvisioningJsonAsync(IFormFile file)
    {
        if (file.FileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
        {
            using var stream = file.OpenReadStream();
            using var archive = new ZipArchive(stream, ZipArchiveMode.Read);
            var entry = archive.GetEntry("edge-provisioning.json")
                        ?? throw new InvalidOperationException("ZIP không chứa file edge-provisioning.json.");
            using var entryStream = entry.Open();
            using var reader = new StreamReader(entryStream);
            return await reader.ReadToEndAsync();
        }

        using var jsonStream = file.OpenReadStream();
        using var jsonReader = new StreamReader(jsonStream);
        return await jsonReader.ReadToEndAsync();
    }

    private class ParsedProvisioningJson
    {
        public string SchemaVersion = "1.0";
        public string? NodeId;
        public string? VesselImo;
        public string? VesselName;
        public Guid? VesselId;
        public string? ShoreBaseUrl;
        public string? NodeApiToken;
        public string? SigningKey;
        public int KeyVersion = 1;
        public string ProtocolVersion = "2";
        public bool SecurityEnabled;
        public int BatchSize = 100;
        public int SyncIntervalSec = 30;
        public string? NetworkType = "Shore_WiFi";
    }

    /// <summary>
    /// Validates the edge-provisioning.json structure against the mandatory checklist from the plan.
    /// Returns an error message string if invalid, or null (with <paramref name="parsed"/> populated) if valid.
    /// </summary>
    private static string? ValidateProvisioningJson(JsonElement root, out ParsedProvisioningJson parsed)
    {
        parsed = new ParsedProvisioningJson();

        var schemaVersion = root.TryGetProperty("schemaVersion", out var sv) ? sv.GetString() : null;
        if (string.IsNullOrWhiteSpace(schemaVersion) || !SupportedSchemaVersions.Contains(schemaVersion))
            return $"schemaVersion không hợp lệ hoặc không hỗ trợ (nhận: '{schemaVersion}').";
        parsed.SchemaVersion = schemaVersion;

        if (!root.TryGetProperty("vessel", out var vessel))
            return "Thiếu object 'vessel' trong edge-provisioning.json.";

        parsed.VesselImo = vessel.TryGetProperty("imo", out var imo) ? imo.GetString() : null;
        parsed.VesselName = vessel.TryGetProperty("name", out var name) ? name.GetString() : null;

        var vesselIdStr = vessel.TryGetProperty("shoreVesselId", out var vid) ? vid.GetString() : null;
        if (string.IsNullOrWhiteSpace(vesselIdStr) || !Guid.TryParse(vesselIdStr, out var vesselIdParsed))
            return "vessel.shoreVesselId không phải là GUID hợp lệ.";
        parsed.VesselId = vesselIdParsed;

        if (!root.TryGetProperty("shoreConnection", out var shoreConnection))
            return "Thiếu object 'shoreConnection'.";

        var baseUrl = shoreConnection.TryGetProperty("baseUrl", out var bu) ? bu.GetString() : null;
        if (string.IsNullOrWhiteSpace(baseUrl) || !baseUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            return "shoreConnection.baseUrl phải bắt đầu bằng 'https://'.";
        parsed.ShoreBaseUrl = baseUrl;

        if (!root.TryGetProperty("nodeCredentials", out var creds))
            return "Thiếu object 'nodeCredentials'.";

        parsed.NodeId = creds.TryGetProperty("nodeId", out var nodeIdEl) ? nodeIdEl.GetString() : null;
        if (string.IsNullOrWhiteSpace(parsed.NodeId) || parsed.NodeId.Any(c => char.IsWhiteSpace(c)))
            return "nodeCredentials.nodeId không được rỗng hoặc chứa khoảng trắng.";

        parsed.NodeApiToken = creds.TryGetProperty("nodeApiToken", out var tokenEl) ? tokenEl.GetString() : null;
        if (string.IsNullOrWhiteSpace(parsed.NodeApiToken))
            return "nodeCredentials.nodeApiToken không được rỗng.";

        parsed.SigningKey = creds.TryGetProperty("signingKey", out var keyEl) ? keyEl.GetString() : null;

        var keyVersion = creds.TryGetProperty("keyVersion", out var kv) ? kv.GetInt32() : 0;
        if (keyVersion <= 0)
            return "nodeCredentials.keyVersion phải là số nguyên dương.";
        parsed.KeyVersion = keyVersion;

        var protocolVersion = creds.TryGetProperty("protocolVersion", out var pv) ? pv.GetString() : null;
        if (string.IsNullOrWhiteSpace(protocolVersion) || !SupportedProtocolVersions.Contains(protocolVersion))
            return $"nodeCredentials.protocolVersion không hợp lệ hoặc không hỗ trợ (nhận: '{protocolVersion}').";
        parsed.ProtocolVersion = protocolVersion;

        parsed.SecurityEnabled = creds.TryGetProperty("securityEnabled", out var se) && se.GetBoolean();

        if (root.TryGetProperty("syncPolicy", out var syncPolicy))
        {
            if (syncPolicy.TryGetProperty("batchSize", out var bs) && bs.TryGetInt32(out var bsVal))
                parsed.BatchSize = bsVal;
            if (syncPolicy.TryGetProperty("syncIntervalSeconds", out var si) && si.TryGetInt32(out var siVal))
                parsed.SyncIntervalSec = siVal;
            if (syncPolicy.TryGetProperty("networkType", out var nt))
                parsed.NetworkType = nt.GetString();
        }

        return null;
    }

    private static string Truncate(string value, int maxLength) =>
        value.Length <= maxLength ? value : value[..maxLength];
}
