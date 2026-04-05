namespace ProductApi.DTOs;

public class CreateCargoplanRequest
{
    public Guid? PlanLegId { get; set; }
    public int? Sequence { get; set; }
    public string? OperationType { get; set; }
    public string? CargoType { get; set; }
    public string? CargoDescription { get; set; }
    public double? PlannedQuantity { get; set; }
    public string? Unit { get; set; }
    public string? PortCode { get; set; }
    public string? PortName { get; set; }
    public string? ShipperName { get; set; }
    public string? ConsigneeName { get; set; }
    public string? SpecialRequirements { get; set; }
    public string? Notes { get; set; }
}

public class UpdateCargoplanRequest
{
    public string? CargoType { get; set; }
    public double? PlannedQuantity { get; set; }
    public string? CargoDescription { get; set; }
    public string? Unit { get; set; }
    public string? OperationType { get; set; }
    public string? PortCode { get; set; }
    public string? PortName { get; set; }
    public string? ShipperName { get; set; }
    public string? ConsigneeName { get; set; }
}

public class CreateBunkerplanRequest
{
    public Guid? PlanLegId { get; set; }
    public int? Sequence { get; set; }
    public string? FuelType { get; set; }
    public double? PlannedQuantity { get; set; }
    public string? OperationType { get; set; }
    public string? PortCode { get; set; }
    public string? PortName { get; set; }
    public double? EstimatedCostUsd { get; set; }
    public string? SupplierName { get; set; }
    public string? Notes { get; set; }
}

public class UpdateBunkerplanRequest
{
    public string? FuelType { get; set; }
    public double? PlannedQuantity { get; set; }
    public string? OperationType { get; set; }
    public string? PortCode { get; set; }
    public string? PortName { get; set; }
    public double? EstimatedCostUsd { get; set; }
}

public class CreateCrewchangeplanRequest
{
    public Guid? PlanLegId { get; set; }
    public int? Sequence { get; set; }
    public Guid? CrewMemberId { get; set; }
    public int? RankId { get; set; }
    public string? ChangeType { get; set; }
    public string? PortCode { get; set; }
    public string? PortName { get; set; }
    public DateTime? PlannedDate { get; set; }
    public string? ReplacementReason { get; set; }
    public string? Notes { get; set; }
}

public class UpdateCrewchangeplanRequest
{
    public Guid? CrewMemberId { get; set; }
    public int? RankId { get; set; }
    public string? ChangeType { get; set; }
    public string? PortCode { get; set; }
    public string? PortName { get; set; }
    public DateTime? PlannedDate { get; set; }
}

public class CreateCostestimateRequest
{
    public int? Sequence { get; set; }
    public string? CostCategory { get; set; }
    public string? Description { get; set; }
    public double? EstimatedAmount { get; set; }
    public string? Currency { get; set; }
    public string? Notes { get; set; }
}

public class UpdateCostestimateRequest
{
    public string? CostCategory { get; set; }
    public string? Description { get; set; }
    public double? EstimatedAmount { get; set; }
    public string? Currency { get; set; }
}

public class CreateRevenueestimateRequest
{
    public int? Sequence { get; set; }
    public string? RevenueCategory { get; set; }
    public string? Description { get; set; }
    public double? EstimatedAmount { get; set; }
    public string? Currency { get; set; }
    public string? Notes { get; set; }
}

public class UpdateRevenueestimateRequest
{
    public string? RevenueCategory { get; set; }
    public string? Description { get; set; }
    public double? EstimatedAmount { get; set; }
    public string? Currency { get; set; }
}
