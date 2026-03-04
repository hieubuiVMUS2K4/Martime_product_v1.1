using System.ComponentModel.DataAnnotations;

namespace Maritime.Shared.Interfaces;

/// <summary>
/// Base interface for all entities that participate in Edge↔Shore sync.
/// Provides audit trail, origin tracking, and sync version for conflict detection.
/// </summary>
public interface ISyncableEntity
{
    bool IsSynced { get; set; }
    string OriginNode { get; set; }
    long SyncVersion { get; set; }
    DateTime CreatedAt { get; set; }
    DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Interface for entities with soft-delete support
/// </summary>
public interface ISoftDeletable
{
    bool IsDeleted { get; set; }
    DateTime? DeletedAt { get; set; }
}
