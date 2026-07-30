using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ProductApi.Data;
using ProductApi.Models;
using ProductApi.Security;

namespace ProductApi.Services
{
    /// <summary>
    /// Result returned by Provision/Rotate operations. Never contains plaintext secrets —
    /// per Vessel Provisioning v3 plan, plaintext secrets only ever leave the server inside
    /// the downloadable Provisioning Package ZIP (BuildProvisioningPackageAsync).
    /// </summary>
    public class NodeProvisioningResult
    {
        public string NodeId { get; set; } = string.Empty;
        public DateTime ProvisionedAt { get; set; }
        public string Status { get; set; } = string.Empty;
        public int KeyVersion { get; set; }
    }

    public class EdgeProvisioningPackageBundle
    {
        public byte[] ZipContent { get; set; } = Array.Empty<byte>();
        public string FileName { get; set; } = string.Empty;
    }

    public interface IVesselProvisioningService
    {
        /// <summary>Generate node secrets and persist them encrypted in DB. Does not return plaintext.</summary>
        Task<NodeProvisioningResult> ProvisionNodeAsync(Guid vesselId, string provisionedBy, string? clientIp, string? nodeIdOverride = null);

        /// <summary>Decrypt secrets from DB, build the Provisioning Package ZIP, audit the download.</summary>
        Task<EdgeProvisioningPackageBundle> BuildProvisioningPackageAsync(Guid vesselId, string shoreBaseUrl, string downloadedBy, string? clientIp);

        /// <summary>Generate a brand-new key pair, keep the previous signing key in a grace window, bump KeyVersion.</summary>
        Task<NodeProvisioningResult> RotateKeyAsync(Guid vesselId, string rotatedBy, string? clientIp);

        /// <summary>Used by handshake endpoint / NodeApiTokenMiddleware to resolve a raw bearer token to its node.</summary>
        Task<SyncNodeTracker?> ValidateNodeTokenAsync(string rawToken);
    }

    public class VesselProvisioningService : IVesselProvisioningService
    {
        // Rotated (previous) NodeApiToken/SigningKey remain valid for this long so Edge has time to re-import.
        private static readonly TimeSpan KeyRotationGraceWindow = TimeSpan.FromHours(24);

        private readonly AppDbContext _context;
        private readonly IDataEncryptionService _encryption;
        private readonly ILogger<VesselProvisioningService> _logger;

        public VesselProvisioningService(
            AppDbContext context,
            IDataEncryptionService encryption,
            ILogger<VesselProvisioningService> logger)
        {
            _context = context;
            _encryption = encryption;
            _logger = logger;
        }

        private static string GenerateRandomHex(int byteLength) =>
            Convert.ToHexString(RandomNumberGenerator.GetBytes(byteLength)).ToLowerInvariant();

        private static string Sha256Hex(string value)
        {
            var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
            return Convert.ToHexString(hashBytes).ToLowerInvariant();
        }

        private async Task<(Vessel vessel, SyncNodeTracker node)> LoadVesselAndNodeAsync(Guid vesselId, bool createNodeIfMissing)
        {
            var vessel = await _context.Vessels.AsNoTracking().FirstOrDefaultAsync(v => v.Id == vesselId);
            if (vessel == null)
                throw new InvalidOperationException($"Vessel {vesselId} not found.");

            var node = await _context.SyncNodeTrackers
                .AsTracking()
                .FirstOrDefaultAsync(n => n.ImoNumber == vessel.IMO);

            if (node == null)
            {
                if (!createNodeIfMissing)
                    throw new InvalidOperationException($"No sync node tracker found for vessel IMO {vessel.IMO}.");

                node = new SyncNodeTracker
                {
                    NodeId = $"edge-{vessel.IMO}-main",
                    ShipName = vessel.Name,
                    ImoNumber = vessel.IMO,
                    IsRegistered = false,
                    IsRevoked = false,
                    ProvisioningStatus = "Unknown",
                    KeyVersion = 1,
                    NodeApiTokenVersion = 1,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.SyncNodeTrackers.Add(node);
                await _context.SaveChangesAsync();
            }

            return (vessel, node);
        }

        public async Task<NodeProvisioningResult> ProvisionNodeAsync(Guid vesselId, string provisionedBy, string? clientIp, string? nodeIdOverride = null)
        {
            var (vessel, node) = await LoadVesselAndNodeAsync(vesselId, createNodeIfMissing: true);

            if (node.ProvisioningStatus is "Revoked" or "Disabled")
                throw new InvalidOperationException($"Node {node.NodeId} is {node.ProvisioningStatus} and cannot be (re-)provisioned. Un-revoke it first.");

            var nodeId = string.IsNullOrWhiteSpace(nodeIdOverride) ? $"edge-{vessel.IMO}-main" : nodeIdOverride.Trim();
            var nodeApiToken = GenerateRandomHex(32); // 64 hex chars
            var signingKey = GenerateRandomHex(32);
            var now = DateTime.UtcNow;

            node.NodeId = nodeId;
            node.ShipName = vessel.Name;
            node.ImoNumber = vessel.IMO;
            node.NodeApiToken = _encryption.Encrypt(nodeApiToken);
            node.NodeApiTokenHash = Sha256Hex(nodeApiToken);
            node.NodeApiTokenVersion = 1;
            node.NodeApiTokenRotatedAt = null;
            node.SigningKey = _encryption.Encrypt(signingKey);
            node.KeyVersion = 1;
            node.PreviousSigningKey = null;
            node.PreviousKeyVersion = null;
            node.PreviousKeyGraceUntil = null;
            node.ProvisioningStatus = "Provisioned";
            node.ProvisionedAt = now;
            node.IsRegistered = false;
            node.IsRevoked = false;
            node.UpdatedAt = now;

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Vessel Provisioning: node {NodeId} (vessel {VesselId}, IMO {Imo}) provisioned by {ProvisionedBy} from {ClientIp}",
                nodeId, vesselId, vessel.IMO, provisionedBy, clientIp ?? "unknown");

            return new NodeProvisioningResult
            {
                NodeId = nodeId,
                ProvisionedAt = now,
                Status = node.ProvisioningStatus,
                KeyVersion = node.KeyVersion
            };
        }

        public async Task<EdgeProvisioningPackageBundle> BuildProvisioningPackageAsync(Guid vesselId, string shoreBaseUrl, string downloadedBy, string? clientIp)
        {
            var (vessel, node) = await LoadVesselAndNodeAsync(vesselId, createNodeIfMissing: false);

            if (node.ProvisioningStatus is "Unknown" or "Revoked" or "Disabled")
                throw new InvalidOperationException($"Node {node.NodeId} status is {node.ProvisioningStatus}; provision it first.");

            if (string.IsNullOrWhiteSpace(node.NodeApiToken) || string.IsNullOrWhiteSpace(node.SigningKey))
                throw new InvalidOperationException($"Node {node.NodeId} has no stored credentials to export.");

            var nodeApiToken = _encryption.Decrypt(node.NodeApiToken)
                ?? throw new InvalidOperationException("Failed to decrypt NodeApiToken.");
            var signingKey = _encryption.Decrypt(node.SigningKey)
                ?? throw new InvalidOperationException("Failed to decrypt SigningKey.");

            var generatedAt = DateTime.UtcNow;
            var normalizedBaseUrl = shoreBaseUrl.TrimEnd('/');

            var provisioningJson = new
            {
                schemaVersion = "1.0",
                generatedAt = generatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                generatedBy = "shore",
                vessel = new
                {
                    imo = vessel.IMO,
                    name = vessel.Name,
                    callSign = vessel.CallSign,
                    vesselType = vessel.VesselType,
                    flag = vessel.Flag,
                    grossTonnage = vessel.GrossTonnage,
                    deadWeight = vessel.DeadWeight,
                    shoreVesselId = vessel.Id
                },
                shoreConnection = new { baseUrl = normalizedBaseUrl },
                nodeCredentials = new
                {
                    nodeId = node.NodeId,
                    nodeApiToken,
                    signingKey,
                    keyVersion = node.KeyVersion,
                    protocolVersion = "2",
                    securityEnabled = true
                },
                syncPolicy = new
                {
                    batchSize = 100,
                    syncIntervalSeconds = 30,
                    networkType = "Shore_WiFi"
                }
            };

            var provisioningJsonText = JsonSerializer.Serialize(provisioningJson, new JsonSerializerOptions { WriteIndented = true });

            var downloadCountForHeader = node.ConfigDownloadCount + 1;
            var envShoreSync = BuildEnvShoreSyncContent(vessel, node, normalizedBaseUrl, nodeApiToken, signingKey, generatedAt, downloadCountForHeader);
            var readme = BuildReadmeContent();

            var safeVesselName = string.Concat(vessel.Name.Split(Path.GetInvalidFileNameChars())).Replace(' ', '-');
            var fileName = $"edge-provisioning-{safeVesselName}-{vessel.IMO}-{generatedAt:yyyyMMdd}.zip";

            using var memoryStream = new MemoryStream();
            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, leaveOpen: true))
            {
                await WriteZipEntryAsync(archive, "edge-provisioning.json", provisioningJsonText);
                await WriteZipEntryAsync(archive, ".env.shore-sync", envShoreSync);
                await WriteZipEntryAsync(archive, "README.txt", readme);
            }

            node.ConfigDownloadCount = downloadCountForHeader;
            node.LastConfigDownloadedAt = generatedAt;
            node.LastConfigDownloadedBy = downloadedBy;
            node.LastConfigDownloadedIp = clientIp;
            node.UpdatedAt = generatedAt;
            if (node.ProvisioningStatus == "Provisioned")
                node.ProvisioningStatus = "Downloaded";

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Vessel Provisioning: provisioning package for node {NodeId} downloaded (#{Count}) by {DownloadedBy} from {ClientIp}",
                node.NodeId, node.ConfigDownloadCount, downloadedBy, clientIp ?? "unknown");

            return new EdgeProvisioningPackageBundle
            {
                ZipContent = memoryStream.ToArray(),
                FileName = fileName
            };
        }

        public async Task<NodeProvisioningResult> RotateKeyAsync(Guid vesselId, string rotatedBy, string? clientIp)
        {
            var (vessel, node) = await LoadVesselAndNodeAsync(vesselId, createNodeIfMissing: false);

            if (node.ProvisioningStatus is "Unknown" or "Revoked" or "Disabled")
                throw new InvalidOperationException($"Node {node.NodeId} status is {node.ProvisioningStatus}; cannot rotate keys.");

            var now = DateTime.UtcNow;
            var newNodeApiToken = GenerateRandomHex(32);
            var newSigningKey = GenerateRandomHex(32);

            // Keep the previous signing key valid for a grace window so in-flight signed requests don't break instantly.
            node.PreviousSigningKey = node.SigningKey;
            node.PreviousKeyVersion = node.KeyVersion;
            node.PreviousKeyGraceUntil = now.Add(KeyRotationGraceWindow);

            node.SigningKey = _encryption.Encrypt(newSigningKey);
            node.KeyVersion += 1;

            node.NodeApiToken = _encryption.Encrypt(newNodeApiToken);
            node.NodeApiTokenHash = Sha256Hex(newNodeApiToken);
            node.NodeApiTokenVersion += 1;
            node.NodeApiTokenRotatedAt = now;
            node.LastKeyRotatedAt = now;

            // Per Operational Guards: after rotate, node needs re-import ("Needs Re-import" badge on Shore UI).
            node.ProvisioningStatus = "Downloaded";
            node.UpdatedAt = now;

            await _context.SaveChangesAsync();

            _logger.LogWarning(
                "Vessel Provisioning: keys rotated for node {NodeId} (vessel {VesselId}) by {RotatedBy} from {ClientIp}. New KeyVersion={KeyVersion}. Node now requires re-import.",
                node.NodeId, vesselId, rotatedBy, clientIp ?? "unknown", node.KeyVersion);

            return new NodeProvisioningResult
            {
                NodeId = node.NodeId,
                ProvisionedAt = node.ProvisionedAt ?? now,
                Status = node.ProvisioningStatus,
                KeyVersion = node.KeyVersion
            };
        }

        public async Task<SyncNodeTracker?> ValidateNodeTokenAsync(string rawToken)
        {
            if (string.IsNullOrWhiteSpace(rawToken))
                return null;

            var tokenHash = Sha256Hex(rawToken);
            var node = await _context.SyncNodeTrackers
                .AsNoTracking()
                .FirstOrDefaultAsync(n => n.NodeApiTokenHash == tokenHash);

            if (node == null || node.IsRevoked)
                return null;

            return node;
        }

        private static async Task WriteZipEntryAsync(ZipArchive archive, string entryName, string content)
        {
            var entry = archive.CreateEntry(entryName, CompressionLevel.Optimal);
            await using var entryStream = entry.Open();
            await using var writer = new StreamWriter(entryStream, new UTF8Encoding(false));
            await writer.WriteAsync(content);
        }

        private static string BuildEnvShoreSyncContent(
            Vessel vessel, SyncNodeTracker node, string baseUrl, string nodeApiToken, string signingKey, DateTime generatedAt, int downloadCount)
        {
            var sb = new StringBuilder();
            sb.AppendLine("# ── GENERATED BY SHORE ──────────────────────────────────────");
            sb.AppendLine($"# Vessel: {vessel.Name} | IMO: {vessel.IMO} | Download #{downloadCount}");
            sb.AppendLine($"# Generated: {generatedAt:yyyy-MM-ddTHH:mm:ssZ}");
            sb.AppendLine("# CHỈ CHỨA: Shore connection + Vessel info + Sync security");
            sb.AppendLine("# KHÔNG chứa: DB password, JWT key, port, migration toggle");
            sb.AppendLine("# LEGACY / MANUAL EXPORT ONLY — Managed Mode KHÔNG đọc file này.");
            sb.AppendLine("# ────────────────────────────────────────────────────────────");
            sb.AppendLine();
            sb.AppendLine("# === Shore Connection ===");
            sb.AppendLine($"SHORE_API_URL={baseUrl}");
            sb.AppendLine($"SHORE_API_KEY={nodeApiToken}");
            sb.AppendLine();
            sb.AppendLine("# === Vessel Identity ===");
            sb.AppendLine($"Vessel__IMO={vessel.IMO}");
            sb.AppendLine($"Vessel__Name={vessel.Name}");
            sb.AppendLine($"Vessel__CallSign={vessel.CallSign}");
            sb.AppendLine($"Vessel__VesselType={vessel.VesselType}");
            sb.AppendLine($"Vessel__GrossTonnage={vessel.GrossTonnage}");
            sb.AppendLine($"Vessel__DeadWeight={vessel.DeadWeight}");
            sb.AppendLine($"ShoreAPI__VesselId={vessel.Id}");
            sb.AppendLine($"ShoreAPI__BaseUrl={baseUrl}");
            sb.AppendLine($"ShoreAPI__ApiKey={nodeApiToken}");
            sb.AppendLine("ShoreAPI__Enabled=true");
            sb.AppendLine();
            sb.AppendLine("# === Sync Security ===");
            sb.AppendLine($"EDGE_NODE_API_TOKEN={nodeApiToken}");
            sb.AppendLine($"EDGE_SYNC_NODE_ID={node.NodeId}");
            sb.AppendLine($"EDGE_SYNC_SIGNING_KEY={signingKey}");
            sb.AppendLine($"EDGE_SYNC_KEY_VERSION={node.KeyVersion}");
            sb.AppendLine("EDGE_SYNC_SECURITY_ENABLED=true");
            sb.AppendLine("EDGE_SYNC_PROTOCOL_VERSION=2");
            sb.AppendLine($"SyncSecurity__NodeId={node.NodeId}");
            sb.AppendLine($"SyncSecurity__SigningKey={signingKey}");
            sb.AppendLine($"SyncSecurity__KeyVersion={node.KeyVersion}");
            sb.AppendLine("SyncSecurity__Enabled=true");
            sb.AppendLine("SyncSecurity__ProtocolVersion=2");
            return sb.ToString();
        }

        private static string BuildReadmeContent() => """
            CÁCH 1 — Import qua giao diện tàu (KHUYẾN NGHỊ):
              1. Mở trình duyệt trên tàu → Settings → Shore Connection
              2. Upload file này (ZIP) hoặc chỉ file edge-provisioning.json
              3. Bấm "Test Connection" → "Activate"
              → Không cần restart, không cần SSH.

            CÁCH 2 — Thủ công qua Docker (backup):
              1. Mở file .env.shore-sync, copy từng dòng cần thiết vào .env hiện tại của tàu
              2. KHÔNG copy đè toàn bộ file (sẽ mất DB password và các config khác)
              3. docker compose restart backend

            LƯU Ý AN TOÀN:
              - File .env.shore-sync chỉ dành cho Legacy/manual export/debug.
              - Managed Mode (khi đã import + activate qua giao diện Edge) tuyệt đối không đọc file này.
              - Không chia sẻ file ZIP này qua kênh không an toàn (email công khai, chat không mã hóa...).
            """;
    }
}
