using System.ComponentModel.DataAnnotations;
using Maritime.Shared.Models.Crew;

namespace Maritime.Shared.Models.CrewManagement;

// ============================================================
// EXTERNAL REQUEST ENTITIES
// ============================================================

/// <summary>
/// A request sent to an external agency for crew candidates.
/// </summary>
public class ExternalRequest
{
    public Guid Id { get; set; }
    public Guid VesselId { get; set; }
    public Guid? AssignmentId { get; set; }
    public int RankId { get; set; }

    [MaxLength(200)]
    public string? AgencyName { get; set; }
    [MaxLength(200)]
    public string? AgencyEmail { get; set; }

    public int RequiredCount { get; set; } = 1;
    public string? NationalityPreference { get; set; }
    public DateTime? RequiredByDate { get; set; }
    public DateTime? ResponseSlaDate { get; set; }

    [MaxLength(50)]
    public string Status { get; set; } = ExternalRequestStatus.Draft;
    public DateTime? SentAt { get; set; }
    public DateTime? ViewedAt { get; set; }
    public DateTime? ClosedAt { get; set; }

    public string? Notes { get; set; }
    public string? MandatoryDocuments { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    [MaxLength(100)]
    public string? CreatedBy { get; set; }

    // Navigation (Vessel resolved via DbContext in backend, not shared)
    public CrewAssignment? Assignment { get; set; }
    public Rank? Rank { get; set; }
    public List<ExternalCandidate> Candidates { get; set; } = [];
    public List<ExternalRequestMessage> Messages { get; set; } = [];
}

/// <summary>
/// A candidate submitted by an agency in response to an external request.
/// </summary>
public class ExternalCandidate
{
    public Guid Id { get; set; }
    public Guid ExternalRequestId { get; set; }

    [MaxLength(200)]
    public string CandidateName { get; set; } = string.Empty;
    [MaxLength(100)]
    public string? Nationality { get; set; }
    public int? RankId { get; set; }
    [MaxLength(100)]
    public string? ContactEmail { get; set; }
    [MaxLength(50)]
    public string? ContactPhone { get; set; }

    [MaxLength(50)]
    public string Status { get; set; } = ExternalCandidateStatus.Submitted;
    public string? ComplianceResult { get; set; }
    public string? ProfileSummary { get; set; }
    public string? Notes { get; set; }

    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    public string? SubmittedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewedBy { get; set; }

    // Link to created crew member if accepted
    public Guid? LinkedCrewMemberId { get; set; }

    // Navigation
    public ExternalRequest? ExternalRequest { get; set; }
    public Rank? Rank { get; set; }
    public CrewMember? LinkedCrewMember { get; set; }
}

/// <summary>
/// Communication thread for an external request.
/// </summary>
public class ExternalRequestMessage
{
    public Guid Id { get; set; }
    public Guid ExternalRequestId { get; set; }

    [MaxLength(100)]
    public string Author { get; set; } = string.Empty;
    [MaxLength(50)]
    public string? AuthorRole { get; set; } // Coordinator, Agency
    public string Content { get; set; } = string.Empty;
    public DateTime PostedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ExternalRequest? ExternalRequest { get; set; }
}

// ============================================================
// TRAVEL ENTITIES
// ============================================================

/// <summary>
/// A travel request generated from a confirmed assignment.
/// </summary>
public class TravelRequest
{
    public Guid Id { get; set; }
    public Guid AssignmentId { get; set; }
    public Guid CrewMemberId { get; set; }

    [MaxLength(50)]
    public string Status { get; set; } = TravelRequestStatus.Draft;
    [MaxLength(50)]
    public string? TravelType { get; set; } // JoinVessel, LeaveVessel, Repatriation, Transfer

    [MaxLength(200)]
    public string? DeparturePort { get; set; }
    [MaxLength(200)]
    public string? ArrivalPort { get; set; }
    public DateTime? DepartureDate { get; set; }
    public DateTime? ArrivalDate { get; set; }
    public DateTime? ReportingDate { get; set; }

    public string? SpecialRequirements { get; set; }
    public string? BaggageNotes { get; set; }
    public string? VisaRequirements { get; set; }

    [MaxLength(200)]
    public string? VendorName { get; set; }
    [MaxLength(200)]
    public string? BookingReference { get; set; }

    public decimal? EstimatedCost { get; set; }
    [MaxLength(10)]
    public string? Currency { get; set; }

    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    [MaxLength(100)]
    public string? CreatedBy { get; set; }

    // Navigation
    public CrewAssignment? Assignment { get; set; }
    public CrewMember? CrewMember { get; set; }
    public List<TravelSegment> Segments { get; set; } = [];
    public List<TravelStatusHistory> StatusHistory { get; set; } = [];
}

/// <summary>
/// A leg/segment of a travel itinerary.
/// </summary>
public class TravelSegment
{
    public Guid Id { get; set; }
    public Guid TravelRequestId { get; set; }
    public int SequenceOrder { get; set; }

    [MaxLength(50)]
    public string SegmentType { get; set; } = TravelSegmentType.Flight; // Flight, Ground, Ferry, Hotel, Transfer

    [MaxLength(200)]
    public string? Origin { get; set; }
    [MaxLength(200)]
    public string? Destination { get; set; }
    [MaxLength(100)]
    public string? CarrierName { get; set; }
    [MaxLength(50)]
    public string? FlightNumber { get; set; }
    public DateTime? DepartureTime { get; set; }
    public DateTime? ArrivalTime { get; set; }
    [MaxLength(100)]
    public string? ConfirmationNumber { get; set; }
    public string? Notes { get; set; }

    // Navigation
    public TravelRequest? TravelRequest { get; set; }
}

/// <summary>
/// Status history of a travel request.
/// </summary>
public class TravelStatusHistory
{
    public Guid Id { get; set; }
    public Guid TravelRequestId { get; set; }

    [MaxLength(50)]
    public string FromStatus { get; set; } = string.Empty;
    [MaxLength(50)]
    public string ToStatus { get; set; } = string.Empty;
    [MaxLength(100)]
    public string? ChangedBy { get; set; }
    public string? Reason { get; set; }
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public TravelRequest? TravelRequest { get; set; }
}
