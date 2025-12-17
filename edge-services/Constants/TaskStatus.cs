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
    public const string PLANNED = "PLANNED";
    public const string UNDERWAY = "UNDERWAY";
    public const string COMPLETED = "COMPLETED";
    public const string CANCELLED = "CANCELLED";
}
