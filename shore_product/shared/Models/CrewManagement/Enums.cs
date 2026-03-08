namespace Maritime.Shared.Models.CrewManagement;

/// <summary>
/// Crew profile lifecycle status
/// </summary>
public static class CrewStatus
{
    public const string Draft = "Draft";
    public const string Active = "Active";
    public const string Inactive = "Inactive";
    public const string Retired = "Retired";
    public const string Suspended = "Suspended";

    public static readonly string[] All = [Draft, Active, Inactive, Retired, Suspended];
}

/// <summary>
/// Crew pool availability status
/// </summary>
public static class PoolStatus
{
    public const string Available = "Available";
    public const string Assigned = "Assigned";
    public const string OnLeave = "OnLeave";
    public const string Medical = "Medical";
    public const string Hold = "Hold";
}

/// <summary>
/// Onboarding case lifecycle states
/// </summary>
public static class OnboardingCaseStatus
{
    public const string Draft = "Draft";
    public const string Invited = "Invited";
    public const string InProgress = "InProgress";
    public const string PendingReview = "PendingReview";
    public const string ReturnedForCompletion = "ReturnedForCompletion";
    public const string Approved = "Approved";
    public const string Activated = "Activated";
    public const string Cancelled = "Cancelled";

    public static readonly string[] All = [Draft, Invited, InProgress, PendingReview, ReturnedForCompletion, Approved, Activated, Cancelled];
}

/// <summary>
/// Onboarding checklist item status
/// </summary>
public static class ChecklistItemStatus
{
    public const string Pending = "Pending";
    public const string InProgress = "InProgress";
    public const string Completed = "Completed";
    public const string Waived = "Waived";
    public const string Blocked = "Blocked";
}

/// <summary>
/// Checklist item types
/// </summary>
public static class ChecklistItemType
{
    public const string DocumentUpload = "DocumentUpload";
    public const string DataEntry = "DataEntry";
    public const string Verification = "Verification";
    public const string Confirmation = "Confirmation";
    public const string External = "External";
}

/// <summary>
/// Document submission lifecycle
/// </summary>
public static class DocumentSubmissionStatus
{
    public const string Draft = "Draft";
    public const string Submitted = "Submitted";
    public const string SentForVerification = "SentForVerification";
    public const string UnderReview = "UnderReview";
    public const string Verified = "Verified";
    public const string Rejected = "Rejected";
    public const string Expired = "Expired";
    public const string Archived = "Archived";
    public const string Superseded = "Superseded";

    public static readonly string[] All = [Draft, Submitted, SentForVerification, UnderReview, Verified, Rejected, Expired, Archived, Superseded];
}

/// <summary>
/// Verification action types
/// </summary>
public static class VerificationActionType
{
    public const string Verified = "Verified";
    public const string Rejected = "Rejected";
    public const string RequestReUpload = "RequestReUpload";
    public const string Comment = "Comment";
}

/// <summary>
/// Verification task priority
/// </summary>
public static class VerificationPriority
{
    public const string Normal = "Normal";
    public const string Urgent = "Urgent";
    public const string Critical = "Critical";
}

/// <summary>
/// Audit log action categories
/// </summary>
public static class AuditAction
{
    public const string Create = "Create";
    public const string Update = "Update";
    public const string Delete = "Delete";
    public const string StatusChange = "StatusChange";
    public const string Verify = "Verify";
    public const string Reject = "Reject";
    public const string Approve = "Approve";
    public const string Waive = "Waive";
    public const string Upload = "Upload";
    public const string Download = "Download";
}

// ============================================================
// PHASE 2: COMPLIANCE MATRIX ENUMS
// ============================================================

/// <summary>
/// Rule severity — determines if a rule blocks assignment or is advisory
/// </summary>
public static class RuleSeverity
{
    public const string Blocker = "Blocker";
    public const string Warning = "Warning";
    public const string Info = "Info";

    public static readonly string[] All = [Blocker, Warning, Info];
}

/// <summary>
/// Evaluation stage — when the rule should be checked
/// </summary>
public static class EvaluationStage
{
    public const string Onboarding = "Onboarding";
    public const string PreConfirm = "PreConfirm";
    public const string PreTravel = "PreTravel";
    public const string PreOnboard = "PreOnboard";
    public const string PeriodicReview = "PeriodicReview";

    public static readonly string[] All = [Onboarding, PreConfirm, PreTravel, PreOnboard, PeriodicReview];
}

/// <summary>
/// Compliance evaluation result
/// </summary>
public static class EligibilityResult
{
    public const string Eligible = "Eligible";
    public const string EligibleWithWarnings = "EligibleWithWarnings";
    public const string NotEligible = "NotEligible";
    public const string EligibleByWaiver = "EligibleByWaiver";
}

/// <summary>
/// Compliance evaluation item status
/// </summary>
public static class ComplianceItemResult
{
    public const string Met = "Met";
    public const string MetExpiringSoon = "MetExpiringSoon";
    public const string NotMet = "NotMet";
    public const string Waived = "Waived";
    public const string NotApplicable = "NotApplicable";
}

/// <summary>
/// Compliance waiver status
/// </summary>
public static class WaiverStatus
{
    public const string Pending = "Pending";
    public const string Approved = "Approved";
    public const string Rejected = "Rejected";
    public const string Expired = "Expired";
    public const string Revoked = "Revoked";

    public static readonly string[] All = [Pending, Approved, Rejected, Expired, Revoked];
}

// ============================================================
// PHASE 5: PLANNING & ASSIGNMENT ENUMS
// ============================================================

/// <summary>
/// Crew assignment lifecycle states
/// </summary>
public static class AssignmentStatus
{
    public const string Draft = "Draft";
    public const string Proposed = "Proposed";
    public const string PendingCrewConfirmation = "PendingCrewConfirmation";
    public const string Confirmed = "Confirmed";
    public const string TravelInProgress = "TravelInProgress";
    public const string ReadyToJoin = "ReadyToJoin";
    public const string OnBoarded = "OnBoarded";
    public const string Completed = "Completed";
    public const string Cancelled = "Cancelled";
    public const string Declined = "Declined";

    public static readonly string[] All = [Draft, Proposed, PendingCrewConfirmation, Confirmed,
        TravelInProgress, ReadyToJoin, OnBoarded, Completed, Cancelled, Declined];

    public static readonly string[] Active = [Draft, Proposed, PendingCrewConfirmation, Confirmed,
        TravelInProgress, ReadyToJoin, OnBoarded];
}

/// <summary>
/// Assignment conflict types
/// </summary>
public static class ConflictType
{
    public const string DateOverlap = "DateOverlap";
    public const string GapTooShort = "GapTooShort";
    public const string CrewUnavailable = "CrewUnavailable";
    public const string RankMismatch = "RankMismatch";
    public const string ComplianceBlocker = "ComplianceBlocker";
    public const string TravelInfeasible = "TravelInfeasible";
}

/// <summary>
/// Assignment conflict severity
/// </summary>
public static class ConflictSeverity
{
    public const string Blocker = "Blocker";
    public const string Warning = "Warning";
    public const string Info = "Info";
}

/// <summary>
/// Manning position fill status
/// </summary>
public static class PositionFillStatus
{
    public const string Open = "Open";
    public const string Proposed = "Proposed";
    public const string Filled = "Filled";
    public const string Shortage = "Shortage";
}

// ============================================================
// PHASE 6: EXTERNAL REQUESTS & TRAVEL ENUMS
// ============================================================

/// <summary>
/// External request lifecycle states
/// </summary>
public static class ExternalRequestStatus
{
    public const string Draft = "Draft";
    public const string Sent = "Sent";
    public const string Viewed = "Viewed";
    public const string InProgress = "InProgress";
    public const string CandidateSubmitted = "CandidateSubmitted";
    public const string Shortlisted = "Shortlisted";
    public const string Closed = "Closed";
    public const string Cancelled = "Cancelled";

    public static readonly string[] All = [Draft, Sent, Viewed, InProgress,
        CandidateSubmitted, Shortlisted, Closed, Cancelled];

    public static readonly string[] Active = [Draft, Sent, Viewed, InProgress,
        CandidateSubmitted, Shortlisted];
}

/// <summary>
/// External candidate status
/// </summary>
public static class ExternalCandidateStatus
{
    public const string Submitted = "Submitted";
    public const string UnderReview = "UnderReview";
    public const string Shortlisted = "Shortlisted";
    public const string Accepted = "Accepted";
    public const string Rejected = "Rejected";
    public const string Withdrawn = "Withdrawn";
}

/// <summary>
/// Travel request lifecycle states
/// </summary>
public static class TravelRequestStatus
{
    public const string Draft = "Draft";
    public const string Pending = "Pending";
    public const string BookingInProgress = "BookingInProgress";
    public const string Booked = "Booked";
    public const string InTransit = "InTransit";
    public const string Completed = "Completed";
    public const string Cancelled = "Cancelled";
    public const string Reissued = "Reissued";

    public static readonly string[] All = [Draft, Pending, BookingInProgress, Booked,
        InTransit, Completed, Cancelled, Reissued];

    public static readonly string[] Active = [Draft, Pending, BookingInProgress, Booked, InTransit];
}

/// <summary>
/// Travel segment type
/// </summary>
public static class TravelSegmentType
{
    public const string Flight = "Flight";
    public const string Ground = "Ground";
    public const string Ferry = "Ferry";
    public const string Hotel = "Hotel";
    public const string Transfer = "Transfer";
}
