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
    // PMS lifecycle statuses
    public const string SCHEDULED = "SCHEDULED";             // Auto-generated, not yet in window
    public const string UPCOMING = "UPCOMING";               // Within DaysBeforeDue / WindowHours — "Sắp đến hạn"
    public const string DUE = "DUE";                         // Due date reached — ready for execution
    public const string OVERDUE = "OVERDUE";                 // Past due date
    
    // Basic statuses
    public const string PENDING = "PENDING";
    public const string IN_PROGRESS = "IN_PROGRESS";
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
/// Constants cho Maintenance operations
/// </summary>
public static class MaintenanceConstants
{
    /// <summary>
    /// Average running hours per day used to estimate calendar dates from running hours.
    /// TODO: Make configurable per ship via ShipConfiguration.
    /// </summary>
    public const double AVERAGE_HOURS_PER_DAY = 10.0;

    /// <summary>
    /// Default window in hours for UPCOMING status when DaysBeforeDue-based calculation
    /// gives a lower value. Minimum 50 running hours buffer.
    /// </summary>
    public const double MINIMUM_UPCOMING_WINDOW_HOURS = 50.0;
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
    public const string APPROVED = "APPROVED";
    public const string READY = "READY";
    public const string UNDERWAY = "UNDERWAY";
    public const string ARRIVED = "ARRIVED";
    public const string COMPLETED = "COMPLETED";
    public const string CANCELLED = "CANCELLED";

    public static readonly HashSet<string> AllStatuses = new()
    {
        PLANNING,
        APPROVED,
        READY,
        UNDERWAY,
        ARRIVED,
        COMPLETED,
        CANCELLED,
    };

    /// <summary>
    /// Valid status transitions matrix.
    /// Key = current status, Value = list of allowed next statuses.
    /// PLANNING → APPROVED, CANCELLED
    /// APPROVED → READY, PLANNING, CANCELLED
    /// READY → UNDERWAY, PLANNING, CANCELLED
    /// UNDERWAY → ARRIVED, CANCELLED
    /// ARRIVED → COMPLETED, CANCELLED
    /// COMPLETED → (final state, no transitions)
    /// CANCELLED → PLANNING (reopen only)
    /// </summary>
    public static readonly Dictionary<string, string[]> ValidTransitions = new()
    {
        { PLANNING, new[] { APPROVED, CANCELLED } },
        { APPROVED, new[] { READY, PLANNING, CANCELLED } },
        { READY, new[] { UNDERWAY, PLANNING, CANCELLED } },
        { UNDERWAY, new[] { ARRIVED, CANCELLED } },
        { ARRIVED, new[] { COMPLETED, CANCELLED } },
        { COMPLETED, Array.Empty<string>() },
        { CANCELLED, new[] { PLANNING } },
    };

    /// <summary>
    /// Statuses that allow full editing (CRUD on voyage, port calls, crew)
    /// </summary>
    public static readonly HashSet<string> EditableStatuses = new() { PLANNING, APPROVED, READY };

    /// <summary>
    /// Statuses that allow limited editing (performance data, port call times, crew status changes)
    /// </summary>
    public static readonly HashSet<string> LimitedEditStatuses = new() { UNDERWAY, ARRIVED };

    /// <summary>
    /// Statuses that are read-only (no modifications except status change via valid transition)
    /// </summary>
    public static readonly HashSet<string> ReadOnlyStatuses = new() { COMPLETED, CANCELLED };

    /// <summary>
    /// Statuses that represent an active or just-finished execution window.
    /// </summary>
    public static readonly HashSet<string> CurrentVoyageStatuses = new() { UNDERWAY, ARRIVED };

    /// <summary>
    /// Check if a status transition is valid
    /// </summary>
    public static bool IsValidTransition(string from, string to)
    {
        if (from == to) return true; // no-op is always valid
        return ValidTransitions.TryGetValue(from, out var allowed) && allowed.Contains(to);
    }

    public static bool IsKnownStatus(string status)
    {
        return AllStatuses.Contains(status);
    }

    /// <summary>
    /// Check if a voyage with the given status allows general modifications
    /// </summary>
    public static bool AllowsModification(string status)
    {
        return EditableStatuses.Contains(status) || LimitedEditStatuses.Contains(status);
    }

    public static bool IsCurrentVoyageStatus(string status)
    {
        return CurrentVoyageStatuses.Contains(status);
    }
}

public static class VoyageCharterType
{
    public const string VOYAGE_CHARTER = "VOYAGE_CHARTER";
    public const string TIME_CHARTER = "TIME_CHARTER";
    public const string TIME_CHARTER_TRIP = "TIME_CHARTER_TRIP";
    public const string CONTRACT_OF_AFFREIGHTMENT = "CONTRACT_OF_AFFREIGHTMENT";
    public const string OTHER = "OTHER";

    public static readonly HashSet<string> AllTypes = new()
    {
        VOYAGE_CHARTER,
        TIME_CHARTER,
        TIME_CHARTER_TRIP,
        CONTRACT_OF_AFFREIGHTMENT,
        OTHER,
    };

    public static bool IsKnownType(string charterType)
    {
        return AllTypes.Contains(charterType);
    }
}

// ============================================================
// PHASE 4: FINANCIAL CONSTANTS
// ============================================================

public static class VoyageFinancialStatus
{
    public const string OPEN = "OPEN";
    public const string PENDING_SETTLEMENT = "PENDING_SETTLEMENT";
    public const string SETTLED = "SETTLED";
    public const string CLOSED = "CLOSED";

    public static readonly Dictionary<string, HashSet<string>> ValidTransitions = new()
    {
        { OPEN, new() { PENDING_SETTLEMENT, CLOSED } },
        { PENDING_SETTLEMENT, new() { SETTLED, OPEN } },
        { SETTLED, new() { CLOSED, OPEN } },
        { CLOSED, new() { } }, // final — no transitions
    };

    public static bool IsValidTransition(string from, string to)
        => ValidTransitions.TryGetValue(from, out var targets) && targets.Contains(to);
}

public static class ExpenseRequestStatus
{
    public const string DRAFT = "DRAFT";
    public const string SUBMITTED = "SUBMITTED";
    public const string APPROVED = "APPROVED";
    public const string REJECTED = "REJECTED";
    public const string CANCELLED = "CANCELLED";

    public static readonly Dictionary<string, HashSet<string>> ValidTransitions = new()
    {
        { DRAFT, new() { SUBMITTED, CANCELLED } },
        { SUBMITTED, new() { APPROVED, REJECTED, CANCELLED } },
        { APPROVED, new() { } },
        { REJECTED, new() { DRAFT } },
        { CANCELLED, new() { } },
    };
}

public static class AdvancePaymentStatus
{
    public const string PENDING = "PENDING";
    public const string PAID = "PAID";
    public const string SETTLED = "SETTLED";
    public const string CANCELLED = "CANCELLED";

    public static readonly Dictionary<string, HashSet<string>> ValidTransitions = new()
    {
        { PENDING, new() { PAID, CANCELLED } },
        { PAID, new() { SETTLED } },
        { SETTLED, new() { } },
        { CANCELLED, new() { } },
    };
}

public static class DisbursementStatus
{
    public const string RECORDED = "RECORDED";
    public const string VERIFIED = "VERIFIED";
    public const string PAID = "PAID";
    public const string DISPUTED = "DISPUTED";

    public static readonly Dictionary<string, HashSet<string>> ValidTransitions = new()
    {
        { RECORDED, new() { VERIFIED, DISPUTED } },
        { VERIFIED, new() { PAID } },
        { DISPUTED, new() { VERIFIED, RECORDED } },
        { PAID, new() { } },
    };
}

public static class CostAllocationScope
{
    public const string VESSEL = "VESSEL";
    public const string VOYAGE = "VOYAGE";
    public const string GENERAL = "GENERAL";
}

public static class CostCategoryConstants
{
    public const string FUEL = "FUEL";
    public const string PORT_CHARGES = "PORT_CHARGES";
    public const string CANAL_FEES = "CANAL_FEES";
    public const string CREW = "CREW";
    public const string SUPPLIES = "SUPPLIES";
    public const string INSURANCE = "INSURANCE";
    public const string BROKERAGE = "BROKERAGE";
    public const string MISC = "MISC";
}

public static class RevenueCategoryConstants
{
    public const string FREIGHT = "FREIGHT";
    public const string DEMURRAGE = "DEMURRAGE";
    public const string DISPATCH = "DISPATCH";
    public const string DEADFREIGHT = "DEADFREIGHT";
    public const string MISC = "MISC";
}
