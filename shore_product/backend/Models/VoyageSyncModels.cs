using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Maritime.Shared.Interfaces;

namespace ProductApi.Models;

public class VoyagePlanLeg : ISyncableEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid VoyageId { get; set; }

    public int Sequence { get; set; }

    [MaxLength(30)]
    public string LegType { get; set; } = "PASSAGE";

    [MaxLength(5)]
    public string? FromPortCode { get; set; }

    [MaxLength(100)]
    public string? FromPortName { get; set; }

    [MaxLength(5)]
    public string? ToPortCode { get; set; }

    [MaxLength(100)]
    public string? ToPortName { get; set; }

    public DateTime? PlannedDepartureTime { get; set; }
    public DateTime? PlannedArrivalTime { get; set; }
    public double? PlannedDistance { get; set; }
    public double? PlannedDurationHours { get; set; }
    public double? PlannedAverageSpeed { get; set; }

    [MaxLength(30)]
    public string? CargoActivity { get; set; }

    public bool CrewChangePlanned { get; set; }
    public bool BunkerSupplyPlanned { get; set; }
    public double? PlannedFuelConsumption { get; set; }
    public string? WeatherRoutingNotes { get; set; }
    public string? Notes { get; set; }

    public bool IsSynced { get; set; }
    public long SyncVersion { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(50)]
    public string OriginNode { get; set; } = "SHIP_01";

    [ForeignKey(nameof(VoyageId))]
    public virtual VoyageRecord? Voyage { get; set; }
}

public class VoyageStatusHistory : ISyncableEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid VoyageId { get; set; }

    [MaxLength(20)]
    public string? FromStatus { get; set; }

    [Required]
    [MaxLength(20)]
    public string ToStatus { get; set; } = string.Empty;

    [MaxLength(100)]
    public string ChangedBy { get; set; } = "system";

    public string? Notes { get; set; }
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

    public bool IsSynced { get; set; }
    public long SyncVersion { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(50)]
    public string OriginNode { get; set; } = "SHIP_01";

    [ForeignKey(nameof(VoyageId))]
    public virtual VoyageRecord? Voyage { get; set; }
}

public class VoyageCrewAssignment : ISyncableEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid VoyageId { get; set; }

    [Required]
    public Guid CrewMemberId { get; set; }

    public int? RankId { get; set; }

    [MaxLength(20)]
    public string Role { get; set; } = "REGULAR";

    [MaxLength(5)]
    public string? EmbarkPortCode { get; set; }

    [MaxLength(150)]
    public string? EmbarkPortName { get; set; }

    public DateTime? EmbarkDate { get; set; }

    [MaxLength(5)]
    public string? DisembarkPortCode { get; set; }

    [MaxLength(150)]
    public string? DisembarkPortName { get; set; }

    public DateTime? DisembarkDate { get; set; }

    [MaxLength(20)]
    public string? WatchSchedule { get; set; }

    [MaxLength(20)]
    public string Status { get; set; } = "ASSIGNED";

    [MaxLength(500)]
    public string? Remarks { get; set; }

    public bool IsSynced { get; set; }
    public long SyncVersion { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(50)]
    public string OriginNode { get; set; } = "SHIP_01";

    [ForeignKey(nameof(VoyageId))]
    public virtual VoyageRecord? Voyage { get; set; }

    [ForeignKey(nameof(CrewMemberId))]
    public virtual CrewMember? CrewMember { get; set; }

    [ForeignKey(nameof(RankId))]
    public virtual Rank? Rank { get; set; }
}

public class CargoOperation : ISyncableEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(50)]
    public string OperationId { get; set; } = string.Empty;

    public Guid? VoyageId { get; set; }

    public Guid? VoyagePlanLegId { get; set; }

    [Required]
    [MaxLength(20)]
    public string OperationType { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string CargoType { get; set; } = string.Empty;

    public string? CargoDescription { get; set; }
    public double Quantity { get; set; }

    [MaxLength(20)]
    public string Unit { get; set; } = "MT";

    [MaxLength(100)]
    public string? LoadingPort { get; set; }

    [MaxLength(100)]
    public string? DischargePort { get; set; }

    public DateTime? LoadedAt { get; set; }
    public DateTime? DischargedAt { get; set; }

    [MaxLength(200)]
    public string? Shipper { get; set; }

    [MaxLength(200)]
    public string? Consignee { get; set; }

    [MaxLength(100)]
    public string? BillOfLading { get; set; }

    [MaxLength(500)]
    public string? SealNumbers { get; set; }

    public string? SpecialRequirements { get; set; }

    [MaxLength(20)]
    public string Status { get; set; } = "PLANNED";

    public bool IsSynced { get; set; }
    public long SyncVersion { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(50)]
    public string OriginNode { get; set; } = "SHIP_01";

    [ForeignKey(nameof(VoyageId))]
    public virtual VoyageRecord? Voyage { get; set; }
}

public class VoyageLogEntry : ISyncableEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid? VoyageId { get; set; }
    public Guid? VoyagePlanLegId { get; set; }

    [Required]
    [MaxLength(20)]
    public string EventType { get; set; } = string.Empty;

    [Required]
    public DateTime EventDateTime { get; set; } = DateTime.UtcNow;

    public DateTime? EventDateTimeLocal { get; set; }

    [MaxLength(10)]
    public string? TimeZone { get; set; }

    public double Latitude { get; set; }
    public double Longitude { get; set; }

    [MaxLength(100)]
    public string? PortName { get; set; }

    [MaxLength(10)]
    public string? PortLocode { get; set; }

    [MaxLength(50)]
    public string? PortCountry { get; set; }

    [MaxLength(50)]
    public string? BerthNumber { get; set; }

    public double? DistanceToGo { get; set; }
    public double? DistanceFromLast { get; set; }
    public double? TotalVoyageDistance { get; set; }
    public double? CourseOverGround { get; set; }
    public double? SpeedOverGround { get; set; }

    [MaxLength(100)]
    public string? PilotName { get; set; }

    [MaxLength(100)]
    public string? PilotStation { get; set; }

    [Required]
    [MaxLength(100)]
    public string OfficerOnWatch { get; set; } = string.Empty;

    public string? MasterSignature { get; set; }
    public DateTime? SignedAt { get; set; }

    [MaxLength(1000)]
    public string? Remarks { get; set; }

    public bool IsSynced { get; set; }
    public long SyncVersion { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(50)]
    public string OriginNode { get; set; } = "SHIP_01";

    [ForeignKey(nameof(VoyageId))]
    public virtual VoyageRecord? Voyage { get; set; }
}

public class VoyageCargoPlan : ISyncableEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid VoyageId { get; set; }

    public Guid? PlanLegId { get; set; }

    public int Sequence { get; set; }

    [Required]
    [MaxLength(30)]
    public string OperationType { get; set; } = "LOADING";

    [Required]
    [MaxLength(50)]
    public string CargoType { get; set; } = string.Empty;

    public string? CargoDescription { get; set; }
    public double PlannedQuantity { get; set; }

    [MaxLength(10)]
    public string Unit { get; set; } = "MT";

    [MaxLength(5)]
    public string? PortCode { get; set; }

    [MaxLength(150)]
    public string? PortName { get; set; }

    [MaxLength(200)]
    public string? ShipperName { get; set; }

    [MaxLength(200)]
    public string? ConsigneeName { get; set; }

    public string? SpecialRequirements { get; set; }
    public string? Notes { get; set; }

    public bool IsSynced { get; set; }
    public long SyncVersion { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(50)]
    public string OriginNode { get; set; } = "SHIP_01";

    [ForeignKey(nameof(VoyageId))]
    public virtual VoyageRecord? Voyage { get; set; }

    [ForeignKey(nameof(PlanLegId))]
    public virtual VoyagePlanLeg? PlanLeg { get; set; }
}

public class VoyageBunkerPlan : ISyncableEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid VoyageId { get; set; }

    public Guid? PlanLegId { get; set; }

    public int Sequence { get; set; }

    [Required]
    [MaxLength(20)]
    public string FuelType { get; set; } = "VLSFO";

    public double PlannedQuantity { get; set; }

    [MaxLength(20)]
    public string OperationType { get; set; } = "SUPPLY";

    [MaxLength(5)]
    public string? PortCode { get; set; }

    [MaxLength(150)]
    public string? PortName { get; set; }

    public double? EstimatedCostUsd { get; set; }

    [MaxLength(200)]
    public string? SupplierName { get; set; }

    public string? Notes { get; set; }

    public bool IsSynced { get; set; }
    public long SyncVersion { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(50)]
    public string OriginNode { get; set; } = "SHIP_01";

    [ForeignKey(nameof(VoyageId))]
    public virtual VoyageRecord? Voyage { get; set; }

    [ForeignKey(nameof(PlanLegId))]
    public virtual VoyagePlanLeg? PlanLeg { get; set; }
}

public class VoyageCrewChangePlan : ISyncableEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid VoyageId { get; set; }

    public Guid? PlanLegId { get; set; }
    public int Sequence { get; set; }
    public Guid? CrewMemberId { get; set; }
    public int? RankId { get; set; }

    [Required]
    [MaxLength(20)]
    public string ChangeType { get; set; } = "ROTATION";

    [MaxLength(5)]
    public string? PortCode { get; set; }

    [MaxLength(150)]
    public string? PortName { get; set; }

    public DateTime? PlannedDate { get; set; }

    [MaxLength(200)]
    public string? ReplacementReason { get; set; }

    public string? Notes { get; set; }

    public bool IsSynced { get; set; }
    public long SyncVersion { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(50)]
    public string OriginNode { get; set; } = "SHIP_01";

    [ForeignKey(nameof(VoyageId))]
    public virtual VoyageRecord? Voyage { get; set; }

    [ForeignKey(nameof(PlanLegId))]
    public virtual VoyagePlanLeg? PlanLeg { get; set; }

    [ForeignKey(nameof(CrewMemberId))]
    public virtual CrewMember? CrewMember { get; set; }

    [ForeignKey(nameof(RankId))]
    public virtual Rank? Rank { get; set; }
}

public class VoyageCostEstimate : ISyncableEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid VoyageId { get; set; }

    public int Sequence { get; set; }

    [Required]
    [MaxLength(30)]
    public string CostCategory { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Description { get; set; }

    public double EstimatedAmount { get; set; }

    [MaxLength(3)]
    public string Currency { get; set; } = "USD";

    public string? Notes { get; set; }

    public bool IsSynced { get; set; }
    public long SyncVersion { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(50)]
    public string OriginNode { get; set; } = "SHIP_01";

    [ForeignKey(nameof(VoyageId))]
    public virtual VoyageRecord? Voyage { get; set; }
}

public class VoyageRevenueEstimate : ISyncableEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid VoyageId { get; set; }

    public int Sequence { get; set; }

    [Required]
    [MaxLength(30)]
    public string RevenueCategory { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Description { get; set; }

    public double EstimatedAmount { get; set; }

    [MaxLength(3)]
    public string Currency { get; set; } = "USD";

    public string? Notes { get; set; }

    public bool IsSynced { get; set; }
    public long SyncVersion { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(50)]
    public string OriginNode { get; set; } = "SHIP_01";

    [ForeignKey(nameof(VoyageId))]
    public virtual VoyageRecord? Voyage { get; set; }
}

public class VoyageExpenseRequest : ISyncableEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid VoyageId { get; set; }

    [Required]
    [MaxLength(20)]
    public string RequestNumber { get; set; } = string.Empty;

    [Required]
    [MaxLength(30)]
    public string CostCategory { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string AllocationScope { get; set; } = "VOYAGE";

    [MaxLength(200)]
    public string? Description { get; set; }

    public double RequestedAmount { get; set; }

    [MaxLength(3)]
    public string Currency { get; set; } = "USD";

    public double ExchangeRate { get; set; } = 1.0;
    public double RequestedAmountUsd { get; set; }

    [MaxLength(200)]
    public string? VendorName { get; set; }

    [MaxLength(100)]
    public string? VendorReference { get; set; }

    [MaxLength(10)]
    public string? PortCode { get; set; }

    [MaxLength(100)]
    public string? PortName { get; set; }

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "DRAFT";

    [MaxLength(100)]
    public string? RequestedBy { get; set; }

    public DateTime? RequestedAt { get; set; }

    [MaxLength(100)]
    public string? ApprovedBy { get; set; }

    public DateTime? ApprovedAt { get; set; }
    public double? ApprovedAmount { get; set; }
    public double? ApprovedAmountUsd { get; set; }
    public string? ApprovalNotes { get; set; }
    public string? Notes { get; set; }
    public string? SupportingDocuments { get; set; }

    public bool IsSynced { get; set; }
    public long SyncVersion { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(50)]
    public string OriginNode { get; set; } = "SHIP_01";

    [ForeignKey(nameof(VoyageId))]
    public virtual VoyageRecord? Voyage { get; set; }
}

public class VoyageAdvancePayment : ISyncableEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid VoyageId { get; set; }

    [Required]
    [MaxLength(20)]
    public string AdvanceNumber { get; set; } = string.Empty;

    [Required]
    [MaxLength(30)]
    public string AdvanceType { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Description { get; set; }

    public double Amount { get; set; }

    [MaxLength(3)]
    public string Currency { get; set; } = "USD";

    public double ExchangeRate { get; set; } = 1.0;
    public double AmountUsd { get; set; }

    [MaxLength(200)]
    public string? RecipientName { get; set; }

    [MaxLength(10)]
    public string? PortCode { get; set; }

    [MaxLength(100)]
    public string? PortName { get; set; }

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "PENDING";

    public DateTime? PaidAt { get; set; }

    [MaxLength(100)]
    public string? PaidBy { get; set; }

    [MaxLength(100)]
    public string? PaymentReference { get; set; }

    public double SettledAmount { get; set; }
    public double UnsettledBalance { get; set; }
    public string? Notes { get; set; }

    public bool IsSynced { get; set; }
    public long SyncVersion { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(50)]
    public string OriginNode { get; set; } = "SHIP_01";

    [ForeignKey(nameof(VoyageId))]
    public virtual VoyageRecord? Voyage { get; set; }
}

public class VoyageDisbursement : ISyncableEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid VoyageId { get; set; }

    public Guid? ExpenseRequestId { get; set; }
    public Guid? AdvancePaymentId { get; set; }

    [Required]
    [MaxLength(20)]
    public string DisbursementNumber { get; set; } = string.Empty;

    [Required]
    [MaxLength(30)]
    public string CostCategory { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string AllocationScope { get; set; } = "VOYAGE";

    [MaxLength(200)]
    public string? Description { get; set; }

    public double Amount { get; set; }

    [MaxLength(3)]
    public string Currency { get; set; } = "USD";

    public double ExchangeRate { get; set; } = 1.0;
    public double AmountUsd { get; set; }

    [MaxLength(200)]
    public string? VendorName { get; set; }

    [MaxLength(100)]
    public string? InvoiceNumber { get; set; }

    public DateTime? InvoiceDate { get; set; }
    public DateTime? DueDate { get; set; }

    [MaxLength(10)]
    public string? PortCode { get; set; }

    [MaxLength(100)]
    public string? PortName { get; set; }

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "RECORDED";

    [MaxLength(100)]
    public string? VerifiedBy { get; set; }

    public DateTime? VerifiedAt { get; set; }
    public DateTime? PaidAt { get; set; }

    [MaxLength(100)]
    public string? PaymentReference { get; set; }

    public string? Notes { get; set; }
    public string? SupportingDocuments { get; set; }

    public bool IsSynced { get; set; }
    public long SyncVersion { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(50)]
    public string OriginNode { get; set; } = "SHIP_01";

    [ForeignKey(nameof(VoyageId))]
    public virtual VoyageRecord? Voyage { get; set; }

    [ForeignKey(nameof(ExpenseRequestId))]
    public virtual VoyageExpenseRequest? ExpenseRequest { get; set; }

    [ForeignKey(nameof(AdvancePaymentId))]
    public virtual VoyageAdvancePayment? AdvancePayment { get; set; }
}

public class VoyageActualRevenue : ISyncableEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid VoyageId { get; set; }

    [Required]
    [MaxLength(20)]
    public string RevenueNumber { get; set; } = string.Empty;

    [Required]
    [MaxLength(30)]
    public string RevenueCategory { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Description { get; set; }

    public double Amount { get; set; }

    [MaxLength(3)]
    public string Currency { get; set; } = "USD";

    public double ExchangeRate { get; set; } = 1.0;
    public double AmountUsd { get; set; }

    [MaxLength(200)]
    public string? PayerName { get; set; }

    [MaxLength(100)]
    public string? InvoiceNumber { get; set; }

    public DateTime? InvoiceDate { get; set; }

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "INVOICED";

    public DateTime? ReceivedAt { get; set; }

    [MaxLength(100)]
    public string? PaymentReference { get; set; }

    public string? Notes { get; set; }

    public bool IsSynced { get; set; }
    public long SyncVersion { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(50)]
    public string OriginNode { get; set; } = "SHIP_01";

    [ForeignKey(nameof(VoyageId))]
    public virtual VoyageRecord? Voyage { get; set; }
}

public class VoyageSettlement : ISyncableEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid VoyageId { get; set; }

    [Required]
    [MaxLength(20)]
    public string SettlementNumber { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "DRAFT";

    public double TotalExpenseApproved { get; set; }
    public double TotalAdvanced { get; set; }
    public double TotalDisbursed { get; set; }
    public double TotalRevenue { get; set; }
    public double NetResult { get; set; }
    public double AdvanceBalance { get; set; }
    public double? FinalSettlementAmount { get; set; }
    public string? Summary { get; set; }

    [MaxLength(100)]
    public string? PreparedBy { get; set; }

    public DateTime? PreparedAt { get; set; }

    [MaxLength(100)]
    public string? ReviewedBy { get; set; }

    public DateTime? ReviewedAt { get; set; }

    [MaxLength(100)]
    public string? ApprovedBy { get; set; }

    public DateTime? ApprovedAt { get; set; }
    public string? ApprovalNotes { get; set; }
    public string? Notes { get; set; }

    public bool IsSynced { get; set; }
    public long SyncVersion { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(50)]
    public string OriginNode { get; set; } = "SHIP_01";

    [ForeignKey(nameof(VoyageId))]
    public virtual VoyageRecord? Voyage { get; set; }
}