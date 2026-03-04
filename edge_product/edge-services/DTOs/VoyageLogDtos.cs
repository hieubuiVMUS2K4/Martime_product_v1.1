namespace MaritimeEdge.DTOs;

/// <summary>
/// Voyage Log Event Types - Các loại sự kiện hành trình
/// </summary>
public static class VoyageLogEventTypes
{
    // Port Events
    public const string Departure = "DEP";           // Rời cảng
    public const string Arrival = "ARR";             // Cập cảng
    public const string AnchorDrop = "ANCHOR_DROP";  // Thả neo
    public const string AnchorUp = "ANCHOR_UP";      // Kéo neo
    
    // Passage Events  
    public const string COSP = "COSP";               // Commencement of Sea Passage
    public const string EOSP = "EOSP";               // End of Sea Passage
    public const string NoonPosition = "NOON";       // Vị trí trưa
    
    // Pilot Events
    public const string PilotOn = "PILOT_ON";        // Hoa tiêu lên tàu
    public const string PilotOff = "PILOT_OFF";      // Hoa tiêu rời tàu
    
    // Special Events
    public const string Drift = "DRIFT";             // Trôi dạt
    public const string Deviation = "DEVIATION";     // Đổi hành trình
    
    public static readonly Dictionary<string, VoyageLogEventInfo> EventInfoMap = new()
    {
        { Departure, new("DEP", "Departure", "Rời cảng", "🚢", "#22c55e", true) },
        { Arrival, new("ARR", "Arrival", "Cập cảng", "⚓", "#3b82f6", true) },
        { AnchorDrop, new("ANCHOR_DROP", "Anchor Drop", "Thả neo", "⚓", "#6b7280", false) },
        { AnchorUp, new("ANCHOR_UP", "Anchor Up", "Kéo neo", "⚓", "#6b7280", false) },
        { COSP, new("COSP", "Commencement of Sea Passage", "Bắt đầu hành trình", "🌊", "#8b5cf6", false) },
        { EOSP, new("EOSP", "End of Sea Passage", "Kết thúc hành trình", "🌊", "#8b5cf6", false) },
        { NoonPosition, new("NOON", "Noon Position", "Vị trí trưa", "☀️", "#eab308", false) },
        { PilotOn, new("PILOT_ON", "Pilot Boarding", "Hoa tiêu lên tàu", "👤", "#f97316", false) },
        { PilotOff, new("PILOT_OFF", "Pilot Disembark", "Hoa tiêu rời tàu", "👤", "#f97316", false) },
        { Drift, new("DRIFT", "Drifting", "Trôi dạt", "⏸️", "#ef4444", false) },
        { Deviation, new("DEVIATION", "Route Deviation", "Đổi hành trình", "↪️", "#f59e0b", false) }
    };
}

/// <summary>
/// Event Info for UI display
/// </summary>
public record VoyageLogEventInfo(
    string Code,
    string NameEn,
    string NameVi,
    string Icon,
    string Color,
    bool RequiresPort
);

/// <summary>
/// DTO for creating a new Voyage Log entry
/// </summary>
public class CreateVoyageLogEntryDto
{
    public Guid? VoyageId { get; set; }
    
    public string EventType { get; set; } = string.Empty;
    public DateTime EventDateTime { get; set; } = DateTime.UtcNow;
    public DateTime? EventDateTimeLocal { get; set; }
    public string? TimeZone { get; set; }
    
    // Position
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    
    // Port Info
    public string? PortName { get; set; }
    public string? PortLocode { get; set; }
    public string? PortCountry { get; set; }
    public string? BerthNumber { get; set; }
    
    // Navigation
    public double? DistanceToGo { get; set; }
    public double? DistanceFromLast { get; set; }
    public double? TotalVoyageDistance { get; set; }
    public double? CourseOverGround { get; set; }
    public double? SpeedOverGround { get; set; }
    
    // Pilot
    public string? PilotName { get; set; }
    public string? PilotStation { get; set; }
    
    // Officer
    public string OfficerOnWatch { get; set; } = string.Empty;
    public string? Remarks { get; set; }
}

/// <summary>
/// DTO for updating a Voyage Log entry
/// </summary>
public class UpdateVoyageLogEntryDto
{
    public string? EventType { get; set; }
    public DateTime? EventDateTime { get; set; }
    public DateTime? EventDateTimeLocal { get; set; }
    public string? TimeZone { get; set; }
    
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    
    public string? PortName { get; set; }
    public string? PortLocode { get; set; }
    public string? PortCountry { get; set; }
    public string? BerthNumber { get; set; }
    
    public double? DistanceToGo { get; set; }
    public double? DistanceFromLast { get; set; }
    public double? TotalVoyageDistance { get; set; }
    public double? CourseOverGround { get; set; }
    public double? SpeedOverGround { get; set; }
    
    public string? PilotName { get; set; }
    public string? PilotStation { get; set; }
    
    public string? OfficerOnWatch { get; set; }
    public string? Remarks { get; set; }
}

/// <summary>
/// DTO for response (includes system fields)
/// </summary>
public class VoyageLogEntryResponseDto
{
    public Guid Id { get; set; }
    public Guid? VoyageId { get; set; }
    
    public string EventType { get; set; } = string.Empty;
    public DateTime EventDateTime { get; set; }
    public DateTime? EventDateTimeLocal { get; set; }
    public string? TimeZone { get; set; }
    
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    
    public string? PortName { get; set; }
    public string? PortLocode { get; set; }
    public string? PortCountry { get; set; }
    public string? BerthNumber { get; set; }
    
    public double? DistanceToGo { get; set; }
    public double? DistanceFromLast { get; set; }
    public double? TotalVoyageDistance { get; set; }
    public double? CourseOverGround { get; set; }
    public double? SpeedOverGround { get; set; }
    
    public string? PilotName { get; set; }
    public string? PilotStation { get; set; }
    
    public string OfficerOnWatch { get; set; } = string.Empty;
    public string? MasterSignature { get; set; }
    public DateTime? SignedAt { get; set; }
    public string? Remarks { get; set; }
    
    public bool IsSynced { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string OriginNode { get; set; } = string.Empty;
}

/// <summary>
/// DTO for signing an entry
/// </summary>
public class SignVoyageLogEntryDto
{
    public string Signature { get; set; } = string.Empty;
    public string? Remarks { get; set; }
}

/// <summary>
/// DTO for pagination query
/// </summary>
public class VoyageLogQueryDto
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? EventType { get; set; }
    public Guid? VoyageId { get; set; }
    public string? PortLocode { get; set; }
}

/// <summary>
/// Paginated response
/// </summary>
public class PaginatedVoyageLogResponse
{
    public List<VoyageLogEntryResponseDto> Data { get; set; } = new();
    public int TotalRecords { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}

/// <summary>
/// Timeline view item (simplified for timeline display)
/// </summary>
public class VoyageLogTimelineItem
{
    public Guid Id { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string EventName { get; set; } = string.Empty;
    public string EventIcon { get; set; } = string.Empty;
    public string EventColor { get; set; } = string.Empty;
    public DateTime EventDateTime { get; set; }
    public string Location { get; set; } = string.Empty; // Port name or coordinates
    public string? Details { get; set; } // Additional info (pilot, distance, etc.)
    public bool IsSigned { get; set; }
}
