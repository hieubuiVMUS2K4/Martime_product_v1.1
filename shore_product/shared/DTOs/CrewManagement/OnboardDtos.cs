namespace Maritime.Shared.DTOs.CrewManagement;

// ============================================================
// ONBOARD EVENT DTOs
// ============================================================

public class OnboardEventDto
{
    public Guid Id { get; set; }
    public Guid CrewMemberId { get; set; }
    public string? CrewName { get; set; }
    public Guid VesselId { get; set; }
    public string? VesselName { get; set; }
    public Guid? AssignmentId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public DateTime EventTimestamp { get; set; }
    public string? PortCode { get; set; }
    public string? PortName { get; set; }
    public string? ConfirmedBy { get; set; }
    public string? ConfirmedByRole { get; set; }
    public string? SignOffReason { get; set; }
    public string? Remarks { get; set; }
    public Guid? OriginalEventId { get; set; }
    public string Source { get; set; } = string.Empty;
    public bool IsSynced { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateOnboardEventRequest
{
    public Guid CrewMemberId { get; set; }
    public Guid VesselId { get; set; }
    public Guid? AssignmentId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public DateTime EventTimestamp { get; set; }
    public string? PortCode { get; set; }
    public string? PortName { get; set; }
    public string? ConfirmedBy { get; set; }
    public string? ConfirmedByRole { get; set; }
    public string? SignOffReason { get; set; }
    public string? Remarks { get; set; }
    public Guid? OriginalEventId { get; set; }
    public string Source { get; set; } = "SHORE";
}

// ============================================================
// CREW ACCESS GRANT DTOs
// ============================================================

public class CrewAccessGrantDto
{
    public Guid Id { get; set; }
    public Guid CrewMemberId { get; set; }
    public string? CrewName { get; set; }
    public Guid VesselId { get; set; }
    public string? VesselName { get; set; }
    public Guid? AssignmentId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public DateTime? GrantedAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? RevokeReason { get; set; }
    public string? GrantedBy { get; set; }
    public string? RevokedBy { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class GrantAccessRequest
{
    public Guid CrewMemberId { get; set; }
    public Guid VesselId { get; set; }
    public Guid? AssignmentId { get; set; }
    public string Module { get; set; } = "All";
    public string? GrantedBy { get; set; }
}

public class RevokeAccessRequest
{
    public string? RevokeReason { get; set; }
    public string? RevokedBy { get; set; }
}

public class SuspendAccessRequest
{
    public string? Reason { get; set; }
    public string? SuspendedBy { get; set; }
}

// ============================================================
// SIGN-ON RECORD DTOs
// ============================================================

public class SignOnRecordDto
{
    public Guid Id { get; set; }
    public Guid CrewMemberId { get; set; }
    public string? CrewName { get; set; }
    public Guid VesselId { get; set; }
    public string? VesselName { get; set; }
    public Guid? AssignmentId { get; set; }
    public int RankId { get; set; }
    public string? RankName { get; set; }
    public DateTime SignOnDate { get; set; }
    public string? PortCode { get; set; }
    public string? PortName { get; set; }
    public string SignedOnBy { get; set; } = string.Empty;
    public string? Remarks { get; set; }
    public Guid? OnboardEventId { get; set; }
    public string Source { get; set; } = string.Empty;
    public bool IsSynced { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateSignOnRequest
{
    public Guid CrewMemberId { get; set; }
    public Guid VesselId { get; set; }
    public Guid? AssignmentId { get; set; }
    public int RankId { get; set; }
    public DateTime SignOnDate { get; set; }
    public string? PortCode { get; set; }
    public string? PortName { get; set; }
    public string SignedOnBy { get; set; } = string.Empty;
    public string? Remarks { get; set; }
    public Guid? OnboardEventId { get; set; }
    public string Source { get; set; } = "SHORE";
}

// ============================================================
// SIGN-OFF RECORD DTOs
// ============================================================

public class SignOffRecordDto
{
    public Guid Id { get; set; }
    public Guid CrewMemberId { get; set; }
    public string? CrewName { get; set; }
    public Guid VesselId { get; set; }
    public string? VesselName { get; set; }
    public Guid? AssignmentId { get; set; }
    public int RankId { get; set; }
    public string? RankName { get; set; }
    public DateTime SignOffDate { get; set; }
    public string? PortCode { get; set; }
    public string? PortName { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? ReasonDetail { get; set; }
    public string SignedOffBy { get; set; } = string.Empty;
    public string? Remarks { get; set; }
    public Guid? OnboardEventId { get; set; }
    public Guid? SignOnRecordId { get; set; }
    public string Source { get; set; } = string.Empty;
    public bool IsSynced { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateSignOffRequest
{
    public Guid CrewMemberId { get; set; }
    public Guid VesselId { get; set; }
    public Guid? AssignmentId { get; set; }
    public int RankId { get; set; }
    public DateTime SignOffDate { get; set; }
    public string? PortCode { get; set; }
    public string? PortName { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? ReasonDetail { get; set; }
    public string SignedOffBy { get; set; } = string.Empty;
    public string? Remarks { get; set; }
    public Guid? OnboardEventId { get; set; }
    public Guid? SignOnRecordId { get; set; }
    public string Source { get; set; } = "SHORE";
}
