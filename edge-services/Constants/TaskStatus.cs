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
    public const string PENDING = "PENDING";
    public const string IN_PROGRESS = "IN_PROGRESS";
    public const string OVERDUE = "OVERDUE";
    public const string COMPLETED = "COMPLETED";
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
