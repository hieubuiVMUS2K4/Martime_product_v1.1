namespace MaritimeEdge.DTOs;

// ========== PORT DTOs ==========

public class PortDto
{
    public int Id { get; set; }
    public string PortCode { get; set; } = string.Empty;
    public string PortName { get; set; } = string.Empty;
    public string? Country { get; set; }
    public string? CountryCode { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? TimeZone { get; set; }
    public bool IsActive { get; set; }
}

public class CreatePortDto
{
    public string PortCode { get; set; } = string.Empty;
    public string PortName { get; set; } = string.Empty;
    public string? Country { get; set; }
    public string? CountryCode { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? TimeZone { get; set; }
}

public class UpdatePortDto
{
    public string? PortName { get; set; }
    public string? Country { get; set; }
    public string? CountryCode { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? TimeZone { get; set; }
    public bool? IsActive { get; set; }
}

public class PortSearchQuery
{
    public string? Search { get; set; }
    public string? CountryCode { get; set; }
    public bool? IsActive { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

// ========== PORT CALL DTOs ==========

public class PortCallDto
{
    public Guid Id { get; set; }
    public Guid VoyageId { get; set; }
    public int? PortId { get; set; }
    public string? PortCode { get; set; }
    public string PortName { get; set; } = string.Empty;
    public string? Country { get; set; }
    public string CallType { get; set; } = string.Empty;
    public int Sequence { get; set; }
    public DateTime? ArrivalTime { get; set; }
    public DateTime? DepartureTime { get; set; }
    public string? BerthNumber { get; set; }
    public DateTime? PilotOnBoard { get; set; }
    public DateTime? PilotOffBoard { get; set; }
    public double? DraftFore { get; set; }
    public double? DraftAft { get; set; }
    public bool CargoOpsCompleted { get; set; }
    public string? Remarks { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreatePortCallDto
{
    public Guid VoyageId { get; set; }
    public int? PortId { get; set; }
    public string? PortCode { get; set; }
    public string PortName { get; set; } = string.Empty;
    public string? Country { get; set; }
    public string CallType { get; set; } = "ARRIVAL";
    public int? Sequence { get; set; }
    public DateTime? ArrivalTime { get; set; }
    public DateTime? DepartureTime { get; set; }
    public string? BerthNumber { get; set; }
    public DateTime? PilotOnBoard { get; set; }
    public DateTime? PilotOffBoard { get; set; }
    public double? DraftFore { get; set; }
    public double? DraftAft { get; set; }
    public bool CargoOpsCompleted { get; set; }
    public string? Remarks { get; set; }
}

public class UpdatePortCallDto
{
    public int? PortId { get; set; }
    public string? PortCode { get; set; }
    public string? PortName { get; set; }
    public string? Country { get; set; }
    public string? CallType { get; set; }
    public int? Sequence { get; set; }
    public DateTime? ArrivalTime { get; set; }
    public DateTime? DepartureTime { get; set; }
    public string? BerthNumber { get; set; }
    public DateTime? PilotOnBoard { get; set; }
    public DateTime? PilotOffBoard { get; set; }
    public double? DraftFore { get; set; }
    public double? DraftAft { get; set; }
    public bool? CargoOpsCompleted { get; set; }
    public string? Remarks { get; set; }
}

// ========== VOYAGE CREW ASSIGNMENT DTOs ==========

public class VoyageCrewAssignmentDto
{
    public Guid Id { get; set; }
    public Guid VoyageId { get; set; }
    public string? VoyageNumber { get; set; }
    public Guid CrewMemberId { get; set; }
    public string? CrewName { get; set; }
    public string? CrewId { get; set; }
    public int? RankId { get; set; }
    public string? RankName { get; set; }
    public string Role { get; set; } = string.Empty;
    public string? EmbarkPortCode { get; set; }
    public string? EmbarkPortName { get; set; }
    public DateTime? EmbarkDate { get; set; }
    public string? DisembarkPortCode { get; set; }
    public string? DisembarkPortName { get; set; }
    public DateTime? DisembarkDate { get; set; }
    public string? WatchSchedule { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Remarks { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateVoyageCrewAssignmentDto
{
    public Guid VoyageId { get; set; }
    public Guid CrewMemberId { get; set; }
    public int? RankId { get; set; }
    public string Role { get; set; } = "REGULAR";
    public string? EmbarkPortCode { get; set; }
    public string? EmbarkPortName { get; set; }
    public DateTime? EmbarkDate { get; set; }
    public string? WatchSchedule { get; set; }
    public string? Remarks { get; set; }
}

public class UpdateVoyageCrewAssignmentDto
{
    public int? RankId { get; set; }
    public string? Role { get; set; }
    public string? EmbarkPortCode { get; set; }
    public string? EmbarkPortName { get; set; }
    public DateTime? EmbarkDate { get; set; }
    public string? DisembarkPortCode { get; set; }
    public string? DisembarkPortName { get; set; }
    public DateTime? DisembarkDate { get; set; }
    public string? WatchSchedule { get; set; }
    public string? Status { get; set; }
    public string? Remarks { get; set; }
}

public class BulkAssignCrewDto
{
    public Guid VoyageId { get; set; }
    public List<Guid> CrewMemberIds { get; set; } = new();
    public string? EmbarkPortCode { get; set; }
    public string? EmbarkPortName { get; set; }
    public DateTime? EmbarkDate { get; set; }
}

// ========== VOYAGE DTOs (Extended) ==========

public class VoyageDetailDto
{
    public Guid Id { get; set; }
    public string VoyageNumber { get; set; } = string.Empty;
    
    // Vessel Info
    public string? VesselIMO { get; set; }
    public string? VesselName { get; set; }
    public string? VesselFlag { get; set; }
    public string? CallSign { get; set; }
    
    // Port Info
    public string? DeparturePort { get; set; }
    public string? DeparturePortCode { get; set; }
    public DateTime? DepartureTime { get; set; }
    public string? ArrivalPort { get; set; }
    public string? ArrivalPortCode { get; set; }
    public DateTime? ArrivalTime { get; set; }
    public string? PreviousPortCode { get; set; }
    public string? PreviousPortName { get; set; }
    
    // Performance
    public string? CargoType { get; set; }
    public double? CargoWeight { get; set; }
    public double? DistanceTraveled { get; set; }
    public double? FuelConsumed { get; set; }
    public double? AverageSpeed { get; set; }
    public string VoyageStatus { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Related data
    public List<PortCallDto> PortCalls { get; set; } = new();
    public List<VoyageCrewAssignmentDto> CrewAssignments { get; set; } = new();
    public int LogEntryCount { get; set; }
    public int CargoOperationCount { get; set; }
}

public class CreateVoyageDto
{
    public string VoyageNumber { get; set; } = string.Empty;
    public string? DeparturePort { get; set; }
    public string? DeparturePortCode { get; set; }
    public DateTime? DepartureTime { get; set; }
    public string? ArrivalPort { get; set; }
    public string? ArrivalPortCode { get; set; }
    public DateTime? ArrivalTime { get; set; }
    public string? PreviousPortCode { get; set; }
    public string? PreviousPortName { get; set; }
    public string? CargoType { get; set; }
    public double? CargoWeight { get; set; }
}

public class UpdateVoyageDto
{
    public string? VoyageNumber { get; set; }
    public string? DeparturePort { get; set; }
    public string? DeparturePortCode { get; set; }
    public DateTime? DepartureTime { get; set; }
    public string? ArrivalPort { get; set; }
    public string? ArrivalPortCode { get; set; }
    public DateTime? ArrivalTime { get; set; }
    public string? PreviousPortCode { get; set; }
    public string? PreviousPortName { get; set; }
    public string? CargoType { get; set; }
    public double? CargoWeight { get; set; }
    public double? DistanceTraveled { get; set; }
    public double? FuelConsumed { get; set; }
    public double? AverageSpeed { get; set; }
    public string? VoyageStatus { get; set; }
}

/// <summary>
/// FAL Form 5 - Crew List export format
/// </summary>
public class FalForm5Dto
{
    public string VoyageNumber { get; set; } = string.Empty;
    public string? VesselName { get; set; }
    public string? VesselIMO { get; set; }
    public string? VesselFlag { get; set; }
    public string? CallSign { get; set; }
    public string? PortOfArrival { get; set; }
    public string? PortOfArrivalCode { get; set; }
    public DateTime? DateOfArrival { get; set; }
    public string? ArrivedFrom { get; set; }
    public List<FalCrewEntry> CrewList { get; set; } = new();
}

public class FalCrewEntry
{
    public int No { get; set; }
    public string? FullName { get; set; }
    public string? Rank { get; set; }
    public string? Nationality { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? PlaceOfBirth { get; set; }
    public string? TravelDocumentType { get; set; }
    public string? TravelDocumentNumber { get; set; }
}

// ========== CARGO OPERATION DTOs ==========

public class CargoOperationDto
{
    public Guid Id { get; set; }
    public string OperationId { get; set; } = string.Empty;
    public Guid? VoyageId { get; set; }
    public string OperationType { get; set; } = string.Empty;
    public string CargoType { get; set; } = string.Empty;
    public string? CargoDescription { get; set; }
    public double Quantity { get; set; }
    public string Unit { get; set; } = "MT";
    public string? LoadingPort { get; set; }
    public string? DischargePort { get; set; }
    public DateTime? LoadedAt { get; set; }
    public DateTime? DischargedAt { get; set; }
    public string? Shipper { get; set; }
    public string? Consignee { get; set; }
    public string? BillOfLading { get; set; }
    public string? SealNumbers { get; set; }
    public string? SpecialRequirements { get; set; }
    public string Status { get; set; } = "PLANNED";
    public DateTime CreatedAt { get; set; }
}

public class CreateCargoOperationDto
{
    public Guid? VoyageId { get; set; }
    public string OperationType { get; set; } = "LOADING";
    public string CargoType { get; set; } = "GENERAL";
    public string? CargoDescription { get; set; }
    public double Quantity { get; set; }
    public string Unit { get; set; } = "MT";
    public string? LoadingPort { get; set; }
    public string? DischargePort { get; set; }
    public DateTime? LoadedAt { get; set; }
    public DateTime? DischargedAt { get; set; }
    public string? Shipper { get; set; }
    public string? Consignee { get; set; }
    public string? BillOfLading { get; set; }
    public string? SealNumbers { get; set; }
    public string? SpecialRequirements { get; set; }
}

public class UpdateCargoOperationDto
{
    public string? OperationType { get; set; }
    public string? CargoType { get; set; }
    public string? CargoDescription { get; set; }
    public double? Quantity { get; set; }
    public string? Unit { get; set; }
    public string? LoadingPort { get; set; }
    public string? DischargePort { get; set; }
    public DateTime? LoadedAt { get; set; }
    public DateTime? DischargedAt { get; set; }
    public string? Shipper { get; set; }
    public string? Consignee { get; set; }
    public string? BillOfLading { get; set; }
    public string? SealNumbers { get; set; }
    public string? SpecialRequirements { get; set; }
    public string? Status { get; set; }
}
