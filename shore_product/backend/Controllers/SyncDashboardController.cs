using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using ProductApi.Data;
using ProductApi.Models;
using ProductApi.Security;
using ProductApi.Services.Sync;

namespace ProductApi.Controllers;

/// <summary>
/// Shore Sync Dashboard Controller — provides rich analytics for monitoring
/// sync health, per-ship status, certificate alerts, and data integrity.
/// </summary>
[ApiController]
[Route("api/sync/dashboard")]
[Authorize(Policy = "InternalAccess")]
[EnableRateLimiting("observability")]
public class SyncDashboardController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IDataEncryptionService _dataEncryptionService;
    private readonly ILogger<SyncDashboardController> _logger;

    public SyncDashboardController(
        AppDbContext context,
        IDataEncryptionService dataEncryptionService,
        ILogger<SyncDashboardController> logger)
    {
        _context = context;
        _dataEncryptionService = dataEncryptionService;
        _logger = logger;
    }

    /// <summary>
    /// GET /api/sync/dashboard/overview — Fleet-wide sync overview.
    /// </summary>
    [HttpGet("overview")]
    public async Task<IActionResult> GetOverview()
    {
        try
        {
            var nodes = await _context.SyncNodeTrackers.AsNoTracking().ToListAsync();

            var pendingOutbox = await _context.SyncOutbox
                .Where(o => o.DeliveredAt == null)
                .CountAsync();

            var last24h = DateTime.UtcNow.AddHours(-24);
            var recentSyncLogs = await _context.SyncLogs
                .Where(l => l.ProcessedAt >= last24h)
                .GroupBy(l => l.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            var syncedCrewCount = await _context.CrewMembers
                .Where(c => c.IsSynced)
                .CountAsync();
            var totalCrewCount = await _context.CrewMembers.CountAsync();

            var syncedCertCount = await _context.CrewCertificates
                .Where(c => c.IsSynced)
                .CountAsync();
            var totalCertCount = await _context.CrewCertificates.CountAsync();

            return Ok(new
            {
                serverTime = DateTime.UtcNow,
                fleet = new
                {
                    totalNodes = nodes.Count,
                    onlineNodes = nodes.Count(n => n.IsOnline),
                    offlineNodes = nodes.Count(n => !n.IsOnline),
                },
                outbox = new
                {
                    pendingTotal = pendingOutbox,
                    perNode = nodes.Select(n => new
                    {
                        n.NodeId,
                        n.ShipName,
                        n.PendingOutboxCount,
                        n.IsOnline,
                        n.LastPullAt
                    })
                },
                last24Hours = new
                {
                    success = recentSyncLogs.FirstOrDefault(l => l.Status == "SUCCESS")?.Count ?? 0,
                    conflicts = recentSyncLogs.FirstOrDefault(l => l.Status == "CONFLICT")?.Count ?? 0,
                    failures = recentSyncLogs.FirstOrDefault(l => l.Status == "FAILED")?.Count ?? 0,
                },
                dataCoverage = new
                {
                    crew = new { synced = syncedCrewCount, total = totalCrewCount },
                    certificates = new { synced = syncedCertCount, total = totalCertCount }
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching sync dashboard overview");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// GET /api/sync/dashboard/nodes — Detailed per-ship sync status.
    /// </summary>
    [HttpGet("nodes")]
    public async Task<IActionResult> GetNodes()
    {
        try
        {
            var nodes = await _context.SyncNodeTrackers
                .AsNoTracking()
                .OrderByDescending(n => n.IsOnline)
                .ThenBy(n => n.ShipName)
                .ToListAsync();

            return Ok(nodes.Select(n => new
            {
                n.NodeId,
                n.ShipName,
                n.ImoNumber,
                n.IsOnline,
                n.CurrentNetworkType,
                push = new
                {
                    lastAt = n.LastPushAt,
                    totalReceived = n.TotalReceivedCount,
                    lastBatchSize = n.LastPushBatchSize,
                    lastVersion = n.LastReceivedVersion
                },
                pull = new
                {
                    lastAt = n.LastPullAt,
                    totalDelivered = n.TotalDeliveredCount,
                    pending = n.PendingOutboxCount,
                    lastAckedId = n.LastAcknowledgedId
                },
                health = new
                {
                    lastHeartbeat = n.LastHeartbeatAt,
                    lastSignedRequestAt = n.LastSignedRequestAt,
                    consecutiveFailures = n.ConsecutiveFailures,
                    lastError = n.LastError,
                    lastErrorAt = n.LastErrorAt
                },
                security = new
                {
                    n.IsRegistered,
                    n.IsRevoked,
                    n.KeyVersion,
                    n.LastKeyRotatedAt,
                    n.RevokedAt,
                    n.RevokedReason
                }
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching sync nodes");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// GET /api/sync/dashboard/nodes/{nodeId}/history — Sync history for a specific ship.
    /// </summary>
    [HttpGet("nodes/{nodeId}/history")]
    public async Task<IActionResult> GetNodeHistory(string nodeId, [FromQuery] int limit = 50)
    {
        try
        {
            if (limit > 200) limit = 200;

            var logs = await _context.SyncLogs
                .AsNoTracking()
                .Where(l => l.OriginNode == nodeId || l.OriginNode == "SHORE")
                .OrderByDescending(l => l.ProcessedAt)
                .Take(limit)
                .Select(l => new
                {
                    l.Direction,
                    l.OriginNode,
                    l.TableName,
                    l.RecordKey,
                    l.ActionType,
                    l.Status,
                    l.ConflictDetail,
                    l.ProcessedAt
                })
                .ToListAsync();

            return Ok(logs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching node history for {NodeId}", nodeId);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// GET /api/sync/dashboard/nodes/{nodeId}/security — Security/provisioning state for a node.
    /// </summary>
    [HttpGet("nodes/{nodeId}/security")]
    public async Task<IActionResult> GetNodeSecurity(string nodeId)
    {
        var node = await _context.SyncNodeTrackers
            .AsNoTracking()
            .FirstOrDefaultAsync(n => n.NodeId == nodeId);

        if (node == null)
            return NotFound(new { error = "Node not found" });

        var currentSigningKey = _dataEncryptionService.Decrypt(node.SigningKey);
        var previousSigningKey = _dataEncryptionService.Decrypt(node.PreviousSigningKey);

        return Ok(new
        {
            node.NodeId,
            node.ShipName,
            node.ImoNumber,
            node.IsRegistered,
            node.IsRevoked,
            node.KeyVersion,
            node.LastKeyRotatedAt,
            node.LastSignedRequestAt,
            node.LastAcknowledgedKeyVersion,
            node.LastKeyVersionAcknowledgedAt,
            node.PreviousKeyVersion,
            node.PreviousKeyGraceUntil,
            node.RevokedAt,
            node.RevokedReason,
            canRollbackKey = !string.IsNullOrWhiteSpace(previousSigningKey)
                && node.PreviousKeyVersion.HasValue
                && node.PreviousKeyGraceUntil.HasValue
                && node.PreviousKeyGraceUntil.Value >= DateTime.UtcNow,
            hasSigningKey = !string.IsNullOrWhiteSpace(currentSigningKey)
        });
    }

    /// <summary>
    /// PUT /api/sync/dashboard/nodes/{nodeId}/security — Provision or rotate signing key for a node.
    /// </summary>
    [HttpPut("nodes/{nodeId}/security")]
    public async Task<IActionResult> UpsertNodeSecurity(string nodeId, [FromBody] UpsertSyncNodeSecurityRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.SigningKey))
            return BadRequest(new { error = "signingKey is required" });

        if (request.KeyVersion.HasValue && request.KeyVersion.Value <= 0)
            return BadRequest(new { error = "keyVersion must be a positive integer" });

        var node = await _context.SyncNodeTrackers
            .AsTracking()
            .FirstOrDefaultAsync(n => n.NodeId == nodeId);

        var now = DateTime.UtcNow;

        if (node == null)
        {
            node = new SyncNodeTracker
            {
                NodeId = nodeId,
                CreatedAt = now
            };
            _context.SyncNodeTrackers.Add(node);
        }

        var currentSigningKey = _dataEncryptionService.Decrypt(node.SigningKey);
        var isRotation = !string.IsNullOrWhiteSpace(currentSigningKey)
            && !string.Equals(currentSigningKey, request.SigningKey, StringComparison.Ordinal);

        var encryptedRequestedSigningKey = _dataEncryptionService.Encrypt(request.SigningKey);

        node.ShipName = request.ShipName ?? node.ShipName;
        node.ImoNumber = request.ImoNumber ?? node.ImoNumber;
        node.IsRegistered = true;
        node.IsRevoked = false;
        node.RevokedAt = null;
        node.RevokedReason = null;

        if (isRotation)
        {
            var graceMinutes = Math.Max(1, request.PreviousKeyGraceMinutes ?? 1440);
            node.PreviousSigningKey = _dataEncryptionService.Encrypt(currentSigningKey);
            node.PreviousKeyVersion = node.KeyVersion;
            node.PreviousKeyGraceUntil = now.AddMinutes(graceMinutes);
            node.SigningKey = encryptedRequestedSigningKey;
            node.KeyVersion = request.KeyVersion ?? (node.KeyVersion <= 0 ? 1 : node.KeyVersion + 1);
            node.LastKeyRotatedAt = now;
        }
        else
        {
            node.SigningKey = encryptedRequestedSigningKey;
            if (node.KeyVersion <= 0)
                node.KeyVersion = 1;
            else if (request.KeyVersion.HasValue)
                node.KeyVersion = request.KeyVersion.Value;
            if (node.LastKeyRotatedAt == null)
                node.LastKeyRotatedAt = now;
        }

        if (!node.LastAcknowledgedKeyVersion.HasValue)
            node.LastAcknowledgedKeyVersion = node.KeyVersion;

        node.UpdatedAt = now;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            node.NodeId,
            node.IsRegistered,
            node.IsRevoked,
            node.KeyVersion,
            node.LastKeyRotatedAt,
            node.PreviousKeyVersion,
            node.PreviousKeyGraceUntil,
            node.LastAcknowledgedKeyVersion
        });
    }

    /// <summary>
    /// POST /api/sync/dashboard/nodes/{nodeId}/rollback-key — Roll back to the previous signing key during the grace window.
    /// </summary>
    [HttpPost("nodes/{nodeId}/rollback-key")]
    public async Task<IActionResult> RollbackNodeKey(string nodeId)
    {
        var node = await _context.SyncNodeTrackers
            .AsTracking()
            .FirstOrDefaultAsync(n => n.NodeId == nodeId);

        if (node == null)
            return NotFound(new { error = "Node not found" });

        if (string.IsNullOrWhiteSpace(node.PreviousSigningKey) ||
            !node.PreviousKeyVersion.HasValue ||
            !node.PreviousKeyGraceUntil.HasValue)
        {
            return Conflict(new { error = "No previous signing key is available for rollback" });
        }

        if (node.PreviousKeyGraceUntil.Value < DateTime.UtcNow)
        {
            return Conflict(new { error = "Previous signing key grace window has expired" });
        }

        node.SigningKey = _dataEncryptionService.Encrypt(_dataEncryptionService.Decrypt(node.PreviousSigningKey));
        node.KeyVersion = node.PreviousKeyVersion.Value;
        node.PreviousSigningKey = null;
        node.PreviousKeyVersion = null;
        node.PreviousKeyGraceUntil = null;
        node.LastKeyRotatedAt = DateTime.UtcNow;
        node.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            node.NodeId,
            node.KeyVersion,
            node.LastKeyRotatedAt,
            rolledBack = true
        });
    }

    /// <summary>
    /// POST /api/sync/dashboard/nodes/{nodeId}/revoke — Revoke a node identity.
    /// </summary>
    [HttpPost("nodes/{nodeId}/revoke")]
    public async Task<IActionResult> RevokeNode(string nodeId, [FromBody] RevokeSyncNodeRequest request)
    {
        var node = await _context.SyncNodeTrackers
            .AsTracking()
            .FirstOrDefaultAsync(n => n.NodeId == nodeId);

        if (node == null)
            return NotFound(new { error = "Node not found" });

        node.IsRevoked = true;
        node.RevokedAt = DateTime.UtcNow;
        node.RevokedReason = request.Reason;
        node.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(new { node.NodeId, node.IsRevoked, node.RevokedAt, node.RevokedReason });
    }

    /// <summary>
    /// POST /api/sync/dashboard/nodes/{nodeId}/activate — Reactivate a revoked node.
    /// </summary>
    [HttpPost("nodes/{nodeId}/activate")]
    public async Task<IActionResult> ActivateNode(string nodeId)
    {
        var node = await _context.SyncNodeTrackers
            .AsTracking()
            .FirstOrDefaultAsync(n => n.NodeId == nodeId);

        if (node == null)
            return NotFound(new { error = "Node not found" });

        node.IsRegistered = true;
        node.IsRevoked = false;
        node.RevokedAt = null;
        node.RevokedReason = null;
        node.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(new { node.NodeId, node.IsRegistered, node.IsRevoked });
    }

    /// <summary>
    /// GET /api/sync/dashboard/table-stats — Per-table sync statistics.
    /// </summary>
    [HttpGet("table-stats")]
    public async Task<IActionResult> GetTableStats()
    {
        try
        {
            var stats = await _context.SyncTableStats
                .AsNoTracking()
                .OrderBy(s => s.NodeId)
                .ThenBy(s => s.TableName)
                .ToListAsync();

            // If no cached stats, compute on the fly from sync_logs
            if (stats.Count == 0)
            {
                stats = await ComputeTableStatsAsync();
            }

            return Ok(stats.Select(s => new
            {
                s.NodeId,
                s.TableName,
                s.TotalSynced,
                s.TotalConflicts,
                s.TotalFailed,
                s.LastSyncAt,
                s.SnapshotAt
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching table stats");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// GET /api/sync/dashboard/integrity — Data integrity check: compare counts between shore expectations.
    /// </summary>
    [HttpGet("integrity")]
    public async Task<IActionResult> GetIntegrityReport()
    {
        try
        {
            var crewCount = await _context.CrewMembers.CountAsync();
            var certTypeCount = await _context.CrewCertificateTypes.CountAsync();
            var crewCertCount = await _context.CrewCertificates.CountAsync();
            var countryCount = await _context.Countries.CountAsync();
            var rankCount = await _context.Ranks.CountAsync();

            var unsyncedCrew = await _context.CrewMembers.Where(c => !c.IsSynced).CountAsync();
            var unsyncedCerts = await _context.CrewCertificates.Where(c => !c.IsSynced).CountAsync();

            // Check for orphan certificates (crew_certificate without valid crew_member)
            var orphanCerts = await _context.CrewCertificates
                .Where(cc => !_context.CrewMembers.Any(c => c.Id == cc.CrewMemberId))
                .CountAsync();

            // Check for stale outbox items (older than 7 days, undelivered)
            var staleOutbox = await _context.SyncOutbox
                .Where(o => o.DeliveredAt == null && o.CreatedAt < DateTime.UtcNow.AddDays(-7))
                .CountAsync();

            return Ok(new
            {
                timestamp = DateTime.UtcNow,
                counts = new
                {
                    crewMembers = crewCount,
                    certificateTypes = certTypeCount,
                    crewCertificates = crewCertCount,
                    countries = countryCount,
                    ranks = rankCount
                },
                syncGaps = new
                {
                    unsyncedCrew,
                    unsyncedCerts,
                    orphanCertificates = orphanCerts,
                    staleOutboxItems = staleOutbox
                },
                healthy = orphanCerts == 0 && staleOutbox == 0 && unsyncedCrew < 10
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating integrity report");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// POST /api/sync/dashboard/resync/{nodeId} — Force re-sync all crew/cert data to a specific ship.
    /// </summary>
    [HttpPost("resync/{nodeId}")]
    public async Task<IActionResult> ForceResync(
        string nodeId,
        [FromServices] ISyncOutboxService syncOutbox)
    {
        try
        {
            if (string.IsNullOrEmpty(nodeId))
                return BadRequest(new { error = "nodeId is required" });

            var crewMembers = await _context.CrewMembers
                .Include(c => c.Rank)
                .ToListAsync();

            var certificates = await _context.CrewCertificateTypes.ToListAsync();
            var countries = await _context.Countries.ToListAsync();
            var ranks = await _context.Ranks.ToListAsync();

            int enqueued = 0;

            // Re-send master data
            foreach (var cert in certificates)
            {
                await syncOutbox.EnqueueAsync(nodeId, "certificate", cert.Id.ToString(),
                    Maritime.Shared.Models.Sync.SyncActionType.SNAPSHOT, cert);
                enqueued++;
            }
            foreach (var country in countries)
            {
                await syncOutbox.EnqueueAsync(nodeId, "country", country.Id.ToString(),
                    Maritime.Shared.Models.Sync.SyncActionType.SNAPSHOT, country);
                enqueued++;
            }
            foreach (var rank in ranks)
            {
                await syncOutbox.EnqueueAsync(nodeId, "rank", rank.Id.ToString(),
                    Maritime.Shared.Models.Sync.SyncActionType.SNAPSHOT, rank);
                enqueued++;
            }

            // Re-send crew members
            foreach (var crew in crewMembers)
            {
                await syncOutbox.EnqueueAsync(nodeId, "crew_member", crew.Id.ToString(),
                    Maritime.Shared.Models.Sync.SyncActionType.SNAPSHOT, crew);
                enqueued++;
            }

            // Re-send crew certificates
            var crewCerts = await _context.CrewCertificates
                .Include(cc => cc.Certificate)
                .Include(cc => cc.Country)
                .ToListAsync();

            foreach (var cc in crewCerts)
            {
                await syncOutbox.EnqueueAsync(nodeId, "crew_certificate", cc.Id.ToString(),
                    Maritime.Shared.Models.Sync.SyncActionType.SNAPSHOT, cc);
                enqueued++;
            }

            _logger.LogInformation("Force resync to {NodeId}: {Count} items enqueued", nodeId, enqueued);

            return Ok(new { message = $"Enqueued {enqueued} items for resync to {nodeId}" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during force resync to {NodeId}", nodeId);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    // ── Helpers ──

    private async Task<List<SyncTableStats>> ComputeTableStatsAsync()
    {
        return await _context.SyncLogs
            .GroupBy(l => new { l.OriginNode, l.TableName })
            .Select(g => new SyncTableStats
            {
                NodeId = g.Key.OriginNode,
                TableName = g.Key.TableName,
                TotalSynced = g.Count(l => l.Status == "SUCCESS"),
                TotalConflicts = g.Count(l => l.Status == "CONFLICT"),
                TotalFailed = g.Count(l => l.Status == "FAILED"),
                LastSyncAt = g.Max(l => l.ProcessedAt),
                SnapshotAt = DateTime.UtcNow
            })
            .ToListAsync();
    }
}

public sealed class UpsertSyncNodeSecurityRequest
{
    public string? ShipName { get; set; }
    public string? ImoNumber { get; set; }
    public string SigningKey { get; set; } = string.Empty;
    public int? KeyVersion { get; set; }
    public int? PreviousKeyGraceMinutes { get; set; }
}

public sealed class RevokeSyncNodeRequest
{
    public string? Reason { get; set; }
}
