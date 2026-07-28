using System.ComponentModel.DataAnnotations;

namespace MaritimeEdge.Models;

/// <summary>
/// Vessel Provisioning v3 — Single Source of Truth for Edge sync configuration & identity in
/// Managed Mode. Only one row may have <see cref="IsActive"/> = true at any time.
/// Secrets (<see cref="NodeApiToken"/>, <see cref="SigningKey"/>) are stored encrypted using
/// EDGE_DATA_PROTECTION_KEY (see <c>IEdgeDataEncryptionService</c>) — never plaintext at rest.
/// </summary>
public class EdgeProvisioningProfile
{
    [Key]
    public int Id { get; set; }

    public bool IsActive { get; set; } = false;

    // Node identity
    [MaxLength(50)]
    public string? NodeId { get; set; }

    [MaxLength(20)]
    public string? VesselImo { get; set; }

    [MaxLength(200)]
    public string? VesselName { get; set; }

    public Guid? VesselId { get; set; }

    // Shore connection
    [MaxLength(500)]
    public string? ShoreBaseUrl { get; set; }

    // Credentials — encrypted at rest
    [MaxLength(1000)]
    public string? NodeApiToken { get; set; }

    [MaxLength(1000)]
    public string? SigningKey { get; set; }

    public int KeyVersion { get; set; } = 1;

    [MaxLength(10)]
    public string ProtocolVersion { get; set; } = "2";

    public bool SecurityEnabled { get; set; } = false;

    // Sync policy
    public int BatchSize { get; set; } = 100;

    public int SyncIntervalSec { get; set; } = 30;

    [MaxLength(50)]
    public string? NetworkType { get; set; } = "Shore_WiFi";

    // Metadata
    [MaxLength(20)]
    public string SchemaVersion { get; set; } = "1.0";

    public DateTime ImportedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(200)]
    public string? ImportedFrom { get; set; } // 'zip_upload' | 'json_upload' | 'manual'

    public DateTime? ActivatedAt { get; set; }

    // Handshake
    public DateTime? LastHandshakeAt { get; set; }

    [MaxLength(30)]
    public string? HandshakeStatus { get; set; } // 'never' | 'success' | 'failed'

    [MaxLength(500)]
    public string? LastHandshakeError { get; set; }
}
