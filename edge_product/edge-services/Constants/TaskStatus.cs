namespace MaritimeEdge.Constants;

/// <summary>
/// Constants cho Maintenance Task Status
/// Sử dụng constants thay vì hardcode strings để:
/// - Tránh typo
/// - Dễ bảo trì
/// - IntelliSense support
/// - Refactor dễ dàng
/// </summary>
public static class TaskStatus
{
    // Basic statuses
    public const string PENDING = "PENDING";
    public const string IN_PROGRESS = "IN_PROGRESS";
    public const string OVERDUE = "OVERDUE";
    public const string COMPLETED = "COMPLETED";
    
    // Extended workflow statuses (Kanban board)
    public const string TASK = "TASK";                       // New task, not yet assigned
    public const string PENDING_APPROVAL = "PENDING_APPROVAL"; // HIGH/CRITICAL task awaiting C/E approval
    public const string REJECTED = "REJECTED";               // Task rejected by C/E
    public const string CANCELLED = "CANCELLED";             // Task cancelled
}

/// <summary>
/// Constants cho Task Priority
/// </summary>
public static class TaskPriority
{
    public const string CRITICAL = "CRITICAL";
    public const string HIGH = "HIGH";
    public const string NORMAL = "NORMAL";
    public const string LOW = "LOW";
}

/// <summary>
/// Constants cho Task Category
/// </summary>
public static class TaskCategory
{
    public const string ENGINE = "ENGINE";
    public const string DECK = "DECK";
    public const string SAFETY = "SAFETY";
    public const string ELECTRICAL = "ELECTRICAL";
    public const string NAVIGATION = "NAVIGATION";
    public const string GENERAL = "GENERAL";
}

/// <summary>
/// Constants cho Alarm Severity
/// </summary>
public static class AlarmSeverity
{
    public const string CRITICAL = "CRITICAL";
    public const string HIGH = "HIGH";
    public const string WARNING = "WARNING";
    public const string INFO = "INFO";
}

/// <summary>
/// Constants cho Voyage Status
/// </summary>
public static class VoyageStatus
{
    public const string PLANNING = "PLANNING";
    public const string UNDERWAY = "UNDERWAY";
    public const string COMPLETED = "COMPLETED";
    public const string CANCELLED = "CANCELLED";

    /// <summary>
    /// Valid status transitions matrix.
    /// Key = current status, Value = list of allowed next statuses.
    /// PLANNING → UNDERWAY, CANCELLED
    /// UNDERWAY → COMPLETED, CANCELLED
    /// COMPLETED → (final state, no transitions)
    /// CANCELLED → PLANNING (reopen only)
    /// </summary>
    public static readonly Dictionary<string, string[]> ValidTransitions = new()
    {
        { PLANNING, new[] { UNDERWAY, CANCELLED } },
        { UNDERWAY, new[] { COMPLETED, CANCELLED } },
        { COMPLETED, Array.Empty<string>() },
        { CANCELLED, new[] { PLANNING } },
    };

    /// <summary>
    /// Statuses that allow full editing (CRUD on voyage, port calls, crew)
    /// </summary>
    public static readonly HashSet<string> EditableStatuses = new() { PLANNING };

    /// <summary>
    /// Statuses that allow limited editing (performance data, port call times, crew status changes)
    /// </summary>
    public static readonly HashSet<string> LimitedEditStatuses = new() { UNDERWAY };

    /// <summary>
    /// Statuses that are read-only (no modifications except status change via valid transition)
    /// </summary>
    public static readonly HashSet<string> ReadOnlyStatuses = new() { COMPLETED, CANCELLED };

    /// <summary>
    /// Check if a status transition is valid
    /// </summary>
    public static bool IsValidTransition(string from, string to)
    {
        if (from == to) return true; // no-op is always valid
        return ValidTransitions.TryGetValue(from, out var allowed) && allowed.Contains(to);
    }

    /// <summary>
    /// Check if a voyage with the given status allows general modifications
    /// </summary>
    public static bool AllowsModification(string status)
    {
        return EditableStatuses.Contains(status) || LimitedEditStatuses.Contains(status);
    }
}
