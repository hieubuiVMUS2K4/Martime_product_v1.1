using Microsoft.EntityFrameworkCore;
using ProductApi.Data;
using ProductApi.Models;

namespace ProductApi.Services.Sync;

/// <summary>
/// Runs once at application startup to auto-provision Edge nodes declared under
/// SyncSecurity:Nodes in configuration (appsettings.json or env vars).
///
/// This guarantees that edge nodes can connect immediately after deployment
/// without manual database provisioning via the dashboard API.
///
/// Config format (appsettings.json):
///   "SyncSecurity": {
///     "Nodes": {
///       "8765432": {
///         "SigningKey": "...",
///         "ShipName": "MV MEKONG SPIRIT",
///         "ImoNumber": "8765432",
///         "KeyVersion": 1
///       }
///     }
///   }
///
/// Env-var override (docker-compose):
///   SyncSecurity__Nodes__8765432__SigningKey=...
/// </summary>
public class SyncNodeAutoProvisionService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SyncNodeAutoProvisionService> _logger;

    public SyncNodeAutoProvisionService(
        IServiceProvider serviceProvider,
        IConfiguration configuration,
        ILogger<SyncNodeAutoProvisionService> logger)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var nodesSection = _configuration.GetSection("SyncSecurity:Nodes");
        var nodeIds = nodesSection.GetChildren().Select(c => c.Key).ToList();
        if (nodeIds.Count == 0) return;

        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        foreach (var nodeId in nodeIds)
        {
            try
            {
                var signingKey = _configuration[$"SyncSecurity:Nodes:{nodeId}:SigningKey"];
                var shipName   = _configuration[$"SyncSecurity:Nodes:{nodeId}:ShipName"];
                var imoNumber  = _configuration[$"SyncSecurity:Nodes:{nodeId}:ImoNumber"];
                var keyVersionStr = _configuration[$"SyncSecurity:Nodes:{nodeId}:KeyVersion"];
                int.TryParse(keyVersionStr, out var keyVersion);
                if (keyVersion <= 0) keyVersion = 1;

                if (string.IsNullOrWhiteSpace(signingKey))
                {
                    _logger.LogWarning("SyncSecurity:Nodes:{NodeId}:SigningKey is empty — skipping auto-provision", nodeId);
                    continue;
                }

                var node = await db.SyncNodeTrackers
                    .AsTracking()
                    .FirstOrDefaultAsync(n => n.NodeId == nodeId, cancellationToken);

                var now = DateTime.UtcNow;

                if (node == null)
                {
                    node = new SyncNodeTracker { NodeId = nodeId, CreatedAt = now };
                    db.SyncNodeTrackers.Add(node);
                }

                // Rotate gracefully if the signing key changed
                if (!string.IsNullOrWhiteSpace(node.SigningKey)
                    && !string.Equals(node.SigningKey, signingKey, StringComparison.Ordinal))
                {
                    node.PreviousSigningKey   = node.SigningKey;
                    node.PreviousKeyVersion   = node.KeyVersion;
                    node.PreviousKeyGraceUntil = now.AddHours(24);
                    node.LastKeyRotatedAt      = now;
                }

                node.SigningKey   = signingKey;
                node.KeyVersion   = keyVersion;
                node.ShipName     = shipName ?? node.ShipName;
                node.ImoNumber    = imoNumber ?? node.ImoNumber;
                node.IsRegistered = true;
                node.IsRevoked    = false;
                node.UpdatedAt    = now;

                if (!node.LastAcknowledgedKeyVersion.HasValue)
                    node.LastAcknowledgedKeyVersion = keyVersion;

                await db.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Auto-provisioned sync node {NodeId} ({ShipName})", nodeId, shipName ?? nodeId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to auto-provision sync node {NodeId}", nodeId);
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
