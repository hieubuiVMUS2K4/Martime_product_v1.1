namespace Maritime.Shared.DTOs.CrewManagement;

// ============================================================
// EXTERNAL REQUEST DTOs
// ============================================================

public class ExternalRequestDto
{
    public Guid Id { get; set; }
    public Guid VesselId { get; set; }
    public string? VesselName { get; set; }
    public Guid? AssignmentId { get; set; }
    public int RankId { get; set; }
    public string? RankName { get; set; }
    public string? AgencyName { get; set; }
    public string? AgencyEmail { get; set; }
    public int RequiredCount { get; set; }
    public string? NationalityPreference { get; set; }
    public DateTime? RequiredByDate { get; set; }
    public DateTime? ResponseSlaDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? SentAt { get; set; }
    public DateTime? ViewedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public string? Notes { get; set; }
    public string? MandatoryDocuments { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public int CandidateCount { get; set; }
    public int ShortlistedCount { get; set; }
}

public class CreateExternalRequestRequest
{
    public Guid VesselId { get; set; }
    public Guid? AssignmentId { get; set; }
    public int RankId { get; set; }
    public string? AgencyName { get; set; }
    public string? AgencyEmail { get; set; }
    public int RequiredCount { get; set; } = 1;
    public string? NationalityPreference { get; set; }
    public DateTime? RequiredByDate { get; set; }
    public DateTime? ResponseSlaDate { get; set; }
    public string? Notes { get; set; }
    public string? MandatoryDocuments { get; set; }
}

public class UpdateExternalRequestRequest
{
    public string? AgencyName { get; set; }
    public string? AgencyEmail { get; set; }
    public string? NationalityPreference { get; set; }
    public DateTime? RequiredByDate { get; set; }
    public DateTime? ResponseSlaDate { get; set; }
    public string? Notes { get; set; }
    public string? MandatoryDocuments { get; set; }
}

public class ChangeExternalRequestStatusRequest
{
    public string NewStatus { get; set; } = string.Empty;
    public string? Reason { get; set; }
}

// ============================================================
// EXTERNAL CANDIDATE DTOs
// ============================================================

public class ExternalCandidateDto
{
    public Guid Id { get; set; }
    public Guid ExternalRequestId { get; set; }
    public string CandidateName { get; set; } = string.Empty;
    public string? Nationality { get; set; }
    public int? RankId { get; set; }
    public string? RankName { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ComplianceResult { get; set; }
    public string? ProfileSummary { get; set; }
    public string? Notes { get; set; }
    public DateTime SubmittedAt { get; set; }
    public string? SubmittedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewedBy { get; set; }
    public Guid? LinkedCrewMemberId { get; set; }
}

public class SubmitCandidateRequest
{
    public Guid ExternalRequestId { get; set; }
    public string CandidateName { get; set; } = string.Empty;
    public string? Nationality { get; set; }
    public int? RankId { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? ProfileSummary { get; set; }
    public string? Notes { get; set; }
}

public class ReviewCandidateRequest
{
    public string NewStatus { get; set; } = string.Empty; // Shortlisted, Accepted, Rejected
    public string? Notes { get; set; }
}

// ============================================================
// EXTERNAL REQUEST MESSAGE DTOs
// ============================================================

public class ExternalRequestMessageDto
{
    public Guid Id { get; set; }
    public Guid ExternalRequestId { get; set; }
    public string Author { get; set; } = string.Empty;
    public string? AuthorRole { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime PostedAt { get; set; }
}

public class CreateExternalMessageRequest
{
    public string Content { get; set; } = string.Empty;
}

// ============================================================
// TRAVEL REQUEST DTOs
// ============================================================

public class TravelRequestDto
{
    public Guid Id { get; set; }
    public Guid AssignmentId { get; set; }
    public Guid CrewMemberId { get; set; }
    public string? CrewName { get; set; }
    public string? VesselName { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? TravelType { get; set; }
    public string? DeparturePort { get; set; }
    public string? ArrivalPort { get; set; }
    public DateTime? DepartureDate { get; set; }
    public DateTime? ArrivalDate { get; set; }
    public DateTime? ReportingDate { get; set; }
    public string? SpecialRequirements { get; set; }
    public string? BaggageNotes { get; set; }
    public string? VisaRequirements { get; set; }
    public string? VendorName { get; set; }
    public string? BookingReference { get; set; }
    public decimal? EstimatedCost { get; set; }
    public string? Currency { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public int SegmentCount { get; set; }
    public List<TravelSegmentDto> Segments { get; set; } = [];
}

public class CreateTravelRequestRequest
{
    public Guid AssignmentId { get; set; }
    public string? TravelType { get; set; }
    public string? DeparturePort { get; set; }
    public string? ArrivalPort { get; set; }
    public DateTime? DepartureDate { get; set; }
    public DateTime? ArrivalDate { get; set; }
    public DateTime? ReportingDate { get; set; }
    public string? SpecialRequirements { get; set; }
    public string? BaggageNotes { get; set; }
    public string? VisaRequirements { get; set; }
    public string? Notes { get; set; }
}

public class UpdateTravelRequestRequest
{
    public string? DeparturePort { get; set; }
    public string? ArrivalPort { get; set; }
    public DateTime? DepartureDate { get; set; }
    public DateTime? ArrivalDate { get; set; }
    public DateTime? ReportingDate { get; set; }
    public string? SpecialRequirements { get; set; }
    public string? BaggageNotes { get; set; }
    public string? VisaRequirements { get; set; }
    public string? VendorName { get; set; }
    public string? BookingReference { get; set; }
    public decimal? EstimatedCost { get; set; }
    public string? Currency { get; set; }
    public string? Notes { get; set; }
}

public class ChangeTravelStatusRequest
{
    public string NewStatus { get; set; } = string.Empty;
    public string? Reason { get; set; }
}

// ============================================================
// TRAVEL SEGMENT DTOs
// ============================================================

public class TravelSegmentDto
{
    public Guid Id { get; set; }
    public Guid TravelRequestId { get; set; }
    public int SequenceOrder { get; set; }
    public string SegmentType { get; set; } = string.Empty;
    public string? Origin { get; set; }
    public string? Destination { get; set; }
    public string? CarrierName { get; set; }
    public string? FlightNumber { get; set; }
    public DateTime? DepartureTime { get; set; }
    public DateTime? ArrivalTime { get; set; }
    public string? ConfirmationNumber { get; set; }
    public string? Notes { get; set; }
}

public class CreateTravelSegmentRequest
{
    public Guid TravelRequestId { get; set; }
    public int SequenceOrder { get; set; }
    public string SegmentType { get; set; } = "Flight";
    public string? Origin { get; set; }
    public string? Destination { get; set; }
    public string? CarrierName { get; set; }
    public string? FlightNumber { get; set; }
    public DateTime? DepartureTime { get; set; }
    public DateTime? ArrivalTime { get; set; }
    public string? ConfirmationNumber { get; set; }
    public string? Notes { get; set; }
}

// ============================================================
// TRAVEL STATUS HISTORY DTO
// ============================================================

public class TravelStatusHistoryDto
{
    public Guid Id { get; set; }
    public string FromStatus { get; set; } = string.Empty;
    public string ToStatus { get; set; } = string.Empty;
    public string? ChangedBy { get; set; }
    public string? Reason { get; set; }
    public DateTime ChangedAt { get; set; }
}
