using Microsoft.EntityFrameworkCore;
using ProductApi.Data;
using ProductApi.DTOs;
using Maritime.Shared.DTOs.Sync;
using Maritime.Shared.Models.Sync;
using ProductApi.Models;
using ProductApi.Services.Sync;
using System;
using System.ComponentModel.DataAnnotations;

namespace ProductApi.Services.Voyage;

/// <summary>
/// Exception thrown when a voyage planning entity is not found.
/// </summary>
public class VoyagePlanningNotFoundException : Exception
{
    public VoyagePlanningNotFoundException(string message) : base(message) { }
}

/// <summary>
/// Service for managing Shore-side voyage planning operations.
/// Automatically enqueues changes for synchronization to Edge nodes.
/// </summary>
public interface IVoyagePlanningService
{
    // Cargo Plans
    Task<VoyageCargoPlan> CreateCargoplanAsync(Guid voyageId, CreateCargoplanRequest request);
    Task<VoyageCargoPlan> UpdateCargoplanAsync(Guid id, UpdateCargoplanRequest request);
    Task DeleteCargoplanAsync(Guid id);

    // Bunker Plans
    Task<VoyageBunkerPlan> CreateBunkerplanAsync(Guid voyageId, CreateBunkerplanRequest request);
    Task<VoyageBunkerPlan> UpdateBunkerplanAsync(Guid id, UpdateBunkerplanRequest request);
    Task DeleteBunkerplanAsync(Guid id);

    // Crew Change Plans
    Task<VoyageCrewChangePlan> CreateCrewchangeplanAsync(Guid voyageId, CreateCrewchangeplanRequest request);
    Task<VoyageCrewChangePlan> UpdateCrewchangeplanAsync(Guid id, UpdateCrewchangeplanRequest request);
    Task DeleteCrewchangeplanAsync(Guid id);

    // Cost Estimates
    Task<VoyageCostEstimate> CreateCostestimateAsync(Guid voyageId, CreateCostestimateRequest request);
    Task<VoyageCostEstimate> UpdateCostestimateAsync(Guid id, UpdateCostestimateRequest request);
    Task DeleteCostestimateAsync(Guid id);

    // Revenue Estimates
    Task<VoyageRevenueEstimate> CreateRevenueestimateAsync(Guid voyageId, CreateRevenueestimateRequest request);
    Task<VoyageRevenueEstimate> UpdateRevenueestimateAsync(Guid id, UpdateRevenueestimateRequest request);
    Task DeleteRevenueestimateAsync(Guid id);
}

/// <summary>
/// Implementation of voyage planning service with sync integration.
/// </summary>
public class VoyagePlanningService : IVoyagePlanningService
{
    private readonly AppDbContext _db;
    private readonly ISyncOutboxService _syncOutbox;
    private readonly ILogger<VoyagePlanningService> _logger;

    public VoyagePlanningService(
        AppDbContext db,
        ISyncOutboxService syncOutbox,
        ILogger<VoyagePlanningService> logger)
    {
        _db = db;
        _syncOutbox = syncOutbox;
        _logger = logger;
    }

    // ========== CARGO PLANS ==========

    public async Task<VoyageCargoPlan> CreateCargoplanAsync(Guid voyageId, CreateCargoplanRequest request)
    {
        // Validate voyage exists
        var voyage = await _db.VoyageRecords.FindAsync(voyageId);
        if (voyage == null)
            throw new VoyagePlanningNotFoundException($"Voyage {voyageId} not found");

        var plan = new VoyageCargoPlan
        {
            Id = Guid.NewGuid(),
            VoyageId = voyageId,
            PlanLegId = request.PlanLegId,
            Sequence = request.Sequence ?? 0,
            OperationType = request.OperationType ?? "LOADING",
            CargoType = request.CargoType ?? string.Empty,
            CargoDescription = request.CargoDescription,
            PlannedQuantity = request.PlannedQuantity ?? 0,
            Unit = request.Unit ?? "MT",
            PortCode = request.PortCode,
            PortName = request.PortName,
            ShipperName = request.ShipperName,
            ConsigneeName = request.ConsigneeName,
            SpecialRequirements = request.SpecialRequirements,
            Notes = request.Notes,
            
            // Sync metadata
            IsSynced = false,
            SyncVersion = 0,
            OriginNode = "SHORE",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.VoyageCargoPlans.Add(plan);
        await _db.SaveChangesAsync();

        // Enqueue for sync to all Edge nodes
        await _syncOutbox.EnqueueAsync(
            targetNode: "*",
            tableName: "voyage_cargo_plan",
            recordKey: plan.Id.ToString(),
            action: SyncActionType.CREATE,
            payload: plan
        );

        _logger.LogInformation("[VOYAGE-PLANNING] Created cargo plan {PlanId} for voyage {VoyageId}, " +
            "enqueued to sync outbox", plan.Id, voyageId);

        return plan;
    }

    public async Task<VoyageCargoPlan> UpdateCargoplanAsync(Guid id, UpdateCargoplanRequest request)
    {
        var plan = await _db.VoyageCargoPlans.FirstOrDefaultAsync(p => p.Id == id);
        if (plan == null)
            throw new VoyagePlanningNotFoundException($"Cargo plan {id} not found");

        // Update only provided fields
        if (!string.IsNullOrEmpty(request.CargoType))
            plan.CargoType = request.CargoType;
        if (request.PlannedQuantity.HasValue)
            plan.PlannedQuantity = request.PlannedQuantity.Value;
        if (!string.IsNullOrEmpty(request.CargoDescription))
            plan.CargoDescription = request.CargoDescription;
        if (!string.IsNullOrEmpty(request.Unit))
            plan.Unit = request.Unit;
        if (!string.IsNullOrEmpty(request.OperationType))
            plan.OperationType = request.OperationType;
        if (!string.IsNullOrEmpty(request.PortCode))
            plan.PortCode = request.PortCode;
        if (!string.IsNullOrEmpty(request.PortName))
            plan.PortName = request.PortName;
        if (!string.IsNullOrEmpty(request.ShipperName))
            plan.ShipperName = request.ShipperName;
        if (!string.IsNullOrEmpty(request.ConsigneeName))
            plan.ConsigneeName = request.ConsigneeName;

        // Update sync metadata
        plan.SyncVersion++;
        plan.UpdatedAt = DateTime.UtcNow;
        plan.IsSynced = false;

        await _db.SaveChangesAsync();

        // Enqueue for sync
        await _syncOutbox.EnqueueAsync(
            targetNode: "*",
            tableName: "voyage_cargo_plan",
            recordKey: plan.Id.ToString(),
            action: SyncActionType.UPDATE,
            payload: plan
        );

        _logger.LogInformation("[VOYAGE-PLANNING] Updated cargo plan {PlanId}, version {Version}, " +
            "enqueued to sync outbox", plan.Id, plan.SyncVersion);

        return plan;
    }

    public async Task DeleteCargoplanAsync(Guid id)
    {
        var plan = await _db.VoyageCargoPlans.FindAsync(id);
        if (plan == null)
            throw new VoyagePlanningNotFoundException($"Cargo plan {id} not found");

        _db.VoyageCargoPlans.Remove(plan);
        await _db.SaveChangesAsync();

        // Enqueue for sync
        await _syncOutbox.EnqueueAsync(
            targetNode: "*",
            tableName: "voyage_cargo_plan",
            recordKey: plan.Id.ToString(),
            action: SyncActionType.DELETE,
            payload: plan
        );

        _logger.LogInformation("[VOYAGE-PLANNING] Deleted cargo plan {PlanId}, " +
            "enqueued delete to sync outbox", plan.Id);
    }

    // ========== BUNKER PLANS ==========

    public async Task<VoyageBunkerPlan> CreateBunkerplanAsync(Guid voyageId, CreateBunkerplanRequest request)
    {
        var voyage = await _db.VoyageRecords.FindAsync(voyageId);
        if (voyage == null)
            throw new VoyagePlanningNotFoundException($"Voyage {voyageId} not found");

        var plan = new VoyageBunkerPlan
        {
            Id = Guid.NewGuid(),
            VoyageId = voyageId,
            PlanLegId = request.PlanLegId,
            Sequence = request.Sequence ?? 0,
            FuelType = request.FuelType ?? "VLSFO",
            PlannedQuantity = request.PlannedQuantity ?? 0,
            OperationType = request.OperationType ?? "SUPPLY",
            PortCode = request.PortCode,
            PortName = request.PortName,
            EstimatedCostUsd = request.EstimatedCostUsd,
            SupplierName = request.SupplierName,
            Notes = request.Notes,
            
            // Sync metadata
            IsSynced = false,
            SyncVersion = 0,
            OriginNode = "SHORE",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.VoyageBunkerPlans.Add(plan);
        await _db.SaveChangesAsync();

        await _syncOutbox.EnqueueAsync(
            targetNode: "*",
            tableName: "voyage_bunker_plan",
            recordKey: plan.Id.ToString(),
            action: SyncActionType.CREATE,
            payload: plan
        );

        _logger.LogInformation("[VOYAGE-PLANNING] Created bunker plan {PlanId} for voyage {VoyageId}, " +
            "enqueued to sync outbox", plan.Id, voyageId);

        return plan;
    }

    public async Task<VoyageBunkerPlan> UpdateBunkerplanAsync(Guid id, UpdateBunkerplanRequest request)
    {
        var plan = await _db.VoyageBunkerPlans.FirstOrDefaultAsync(p => p.Id == id);
        if (plan == null)
            throw new VoyagePlanningNotFoundException($"Bunker plan {id} not found");

        // Update only provided fields
        if (!string.IsNullOrEmpty(request.FuelType))
            plan.FuelType = request.FuelType;
        if (request.PlannedQuantity.HasValue)
            plan.PlannedQuantity = request.PlannedQuantity.Value;
        if (!string.IsNullOrEmpty(request.OperationType))
            plan.OperationType = request.OperationType;
        if (!string.IsNullOrEmpty(request.PortCode))
            plan.PortCode = request.PortCode;
        if (!string.IsNullOrEmpty(request.PortName))
            plan.PortName = request.PortName;
        if (request.EstimatedCostUsd.HasValue)
            plan.EstimatedCostUsd = request.EstimatedCostUsd.Value;

        // Update sync metadata
        plan.SyncVersion++;
        plan.UpdatedAt = DateTime.UtcNow;
        plan.IsSynced = false;

        await _db.SaveChangesAsync();

        await _syncOutbox.EnqueueAsync(
            targetNode: "*",
            tableName: "voyage_bunker_plan",
            recordKey: plan.Id.ToString(),
            action: SyncActionType.UPDATE,
            payload: plan
        );

        _logger.LogInformation("[VOYAGE-PLANNING] Updated bunker plan {PlanId}, version {Version}, " +
            "enqueued to sync outbox", plan.Id, plan.SyncVersion);

        return plan;
    }

    public async Task DeleteBunkerplanAsync(Guid id)
    {
        var plan = await _db.VoyageBunkerPlans.FindAsync(id);
        if (plan == null)
            throw new VoyagePlanningNotFoundException($"Bunker plan {id} not found");

        _db.VoyageBunkerPlans.Remove(plan);
        await _db.SaveChangesAsync();

        await _syncOutbox.EnqueueAsync(
            targetNode: "*",
            tableName: "voyage_bunker_plan",
            recordKey: plan.Id.ToString(),
            action: SyncActionType.DELETE,
            payload: plan
        );

        _logger.LogInformation("[VOYAGE-PLANNING] Deleted bunker plan {PlanId}, " +
            "enqueued delete to sync outbox", plan.Id);
    }

    // ========== CREW CHANGE PLANS ==========

    public async Task<VoyageCrewChangePlan> CreateCrewchangeplanAsync(Guid voyageId, CreateCrewchangeplanRequest request)
    {
        var voyage = await _db.VoyageRecords.FindAsync(voyageId);
        if (voyage == null)
            throw new VoyagePlanningNotFoundException($"Voyage {voyageId} not found");

        var plan = new VoyageCrewChangePlan
        {
            Id = Guid.NewGuid(),
            VoyageId = voyageId,
            PlanLegId = request.PlanLegId,
            Sequence = request.Sequence ?? 0,
            CrewMemberId = request.CrewMemberId,
            RankId = request.RankId,
            ChangeType = request.ChangeType ?? "ROTATION",
            PortCode = request.PortCode,
            PortName = request.PortName,
            PlannedDate = request.PlannedDate,
            ReplacementReason = request.ReplacementReason,
            Notes = request.Notes,
            
            // Sync metadata
            IsSynced = false,
            SyncVersion = 0,
            OriginNode = "SHORE",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.VoyageCrewChangePlans.Add(plan);
        await _db.SaveChangesAsync();

        await _syncOutbox.EnqueueAsync(
            targetNode: "*",
            tableName: "voyage_crew_change_plan",
            recordKey: plan.Id.ToString(),
            action: SyncActionType.CREATE,
            payload: plan
        );

        _logger.LogInformation("[VOYAGE-PLANNING] Created crew change plan {PlanId} for voyage {VoyageId}, " +
            "enqueued to sync outbox", plan.Id, voyageId);

        return plan;
    }

    public async Task<VoyageCrewChangePlan> UpdateCrewchangeplanAsync(Guid id, UpdateCrewchangeplanRequest request)
    {
        var plan = await _db.VoyageCrewChangePlans.FirstOrDefaultAsync(p => p.Id == id);
        if (plan == null)
            throw new VoyagePlanningNotFoundException($"Crew change plan {id} not found");

        // Update only provided fields
        if (request.CrewMemberId.HasValue)
            plan.CrewMemberId = request.CrewMemberId.Value;
        if (request.RankId.HasValue)
            plan.RankId = request.RankId.Value;
        if (!string.IsNullOrEmpty(request.ChangeType))
            plan.ChangeType = request.ChangeType;
        if (!string.IsNullOrEmpty(request.PortCode))
            plan.PortCode = request.PortCode;
        if (!string.IsNullOrEmpty(request.PortName))
            plan.PortName = request.PortName;
        if (request.PlannedDate.HasValue)
            plan.PlannedDate = request.PlannedDate.Value;

        // Update sync metadata
        plan.SyncVersion++;
        plan.UpdatedAt = DateTime.UtcNow;
        plan.IsSynced = false;

        await _db.SaveChangesAsync();

        await _syncOutbox.EnqueueAsync(
            targetNode: "*",
            tableName: "voyage_crew_change_plan",
            recordKey: plan.Id.ToString(),
            action: SyncActionType.UPDATE,
            payload: plan
        );

        _logger.LogInformation("[VOYAGE-PLANNING] Updated crew change plan {PlanId}, version {Version}, " +
            "enqueued to sync outbox", plan.Id, plan.SyncVersion);

        return plan;
    }

    public async Task DeleteCrewchangeplanAsync(Guid id)
    {
        var plan = await _db.VoyageCrewChangePlans.FindAsync(id);
        if (plan == null)
            throw new VoyagePlanningNotFoundException($"Crew change plan {id} not found");

        _db.VoyageCrewChangePlans.Remove(plan);
        await _db.SaveChangesAsync();

        await _syncOutbox.EnqueueAsync(
            targetNode: "*",
            tableName: "voyage_crew_change_plan",
            recordKey: plan.Id.ToString(),
            action: SyncActionType.DELETE,
            payload: plan
        );

        _logger.LogInformation("[VOYAGE-PLANNING] Deleted crew change plan {PlanId}, " +
            "enqueued delete to sync outbox", plan.Id);
    }

    // ========== COST ESTIMATES ==========

    public async Task<VoyageCostEstimate> CreateCostestimateAsync(Guid voyageId, CreateCostestimateRequest request)
    {
        var voyage = await _db.VoyageRecords.FindAsync(voyageId);
        if (voyage == null)
            throw new VoyagePlanningNotFoundException($"Voyage {voyageId} not found");

        var estimate = new VoyageCostEstimate
        {
            Id = Guid.NewGuid(),
            VoyageId = voyageId,
            Sequence = request.Sequence ?? 0,
            CostCategory = request.CostCategory ?? string.Empty,
            Description = request.Description,
            EstimatedAmount = request.EstimatedAmount ?? 0,
            Currency = request.Currency ?? "USD",
            Notes = request.Notes,
            
            // Sync metadata
            IsSynced = false,
            SyncVersion = 0,
            OriginNode = "SHORE",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.VoyageCostEstimates.Add(estimate);
        await _db.SaveChangesAsync();

        await _syncOutbox.EnqueueAsync(
            targetNode: "*",
            tableName: "voyage_cost_estimate",
            recordKey: estimate.Id.ToString(),
            action: SyncActionType.CREATE,
            payload: estimate
        );

        _logger.LogInformation("[VOYAGE-PLANNING] Created cost estimate {EstimateId} for voyage {VoyageId}, " +
            "enqueued to sync outbox", estimate.Id, voyageId);

        return estimate;
    }

    public async Task<VoyageCostEstimate> UpdateCostestimateAsync(Guid id, UpdateCostestimateRequest request)
    {
        var estimate = await _db.VoyageCostEstimates.FirstOrDefaultAsync(e => e.Id == id);
        if (estimate == null)
            throw new VoyagePlanningNotFoundException($"Cost estimate {id} not found");

        // Update only provided fields
        if (!string.IsNullOrEmpty(request.CostCategory))
            estimate.CostCategory = request.CostCategory;
        if (!string.IsNullOrEmpty(request.Description))
            estimate.Description = request.Description;
        if (request.EstimatedAmount.HasValue)
            estimate.EstimatedAmount = request.EstimatedAmount.Value;
        if (!string.IsNullOrEmpty(request.Currency))
            estimate.Currency = request.Currency;

        // Update sync metadata
        estimate.SyncVersion++;
        estimate.UpdatedAt = DateTime.UtcNow;
        estimate.IsSynced = false;

        await _db.SaveChangesAsync();

        await _syncOutbox.EnqueueAsync(
            targetNode: "*",
            tableName: "voyage_cost_estimate",
            recordKey: estimate.Id.ToString(),
            action: SyncActionType.UPDATE,
            payload: estimate
        );

        _logger.LogInformation("[VOYAGE-PLANNING] Updated cost estimate {EstimateId}, version {Version}, " +
            "enqueued to sync outbox", estimate.Id, estimate.SyncVersion);

        return estimate;
    }

    public async Task DeleteCostestimateAsync(Guid id)
    {
        var estimate = await _db.VoyageCostEstimates.FindAsync(id);
        if (estimate == null)
            throw new VoyagePlanningNotFoundException($"Cost estimate {id} not found");

        _db.VoyageCostEstimates.Remove(estimate);
        await _db.SaveChangesAsync();

        await _syncOutbox.EnqueueAsync(
            targetNode: "*",
            tableName: "voyage_cost_estimate",
            recordKey: estimate.Id.ToString(),
            action: SyncActionType.DELETE,
            payload: estimate
        );

        _logger.LogInformation("[VOYAGE-PLANNING] Deleted cost estimate {EstimateId}, " +
            "enqueued delete to sync outbox", estimate.Id);
    }

    // ========== REVENUE ESTIMATES ==========

    public async Task<VoyageRevenueEstimate> CreateRevenueestimateAsync(Guid voyageId, CreateRevenueestimateRequest request)
    {
        var voyage = await _db.VoyageRecords.FindAsync(voyageId);
        if (voyage == null)
            throw new VoyagePlanningNotFoundException($"Voyage {voyageId} not found");

        var estimate = new VoyageRevenueEstimate
        {
            Id = Guid.NewGuid(),
            VoyageId = voyageId,
            Sequence = request.Sequence ?? 0,
            RevenueCategory = request.RevenueCategory ?? string.Empty,
            Description = request.Description,
            EstimatedAmount = request.EstimatedAmount ?? 0,
            Currency = request.Currency ?? "USD",
            Notes = request.Notes,
            
            // Sync metadata
            IsSynced = false,
            SyncVersion = 0,
            OriginNode = "SHORE",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.VoyageRevenueEstimates.Add(estimate);
        await _db.SaveChangesAsync();

        await _syncOutbox.EnqueueAsync(
            targetNode: "*",
            tableName: "voyage_revenue_estimate",
            recordKey: estimate.Id.ToString(),
            action: SyncActionType.CREATE,
            payload: estimate
        );

        _logger.LogInformation("[VOYAGE-PLANNING] Created revenue estimate {EstimateId} for voyage {VoyageId}, " +
            "enqueued to sync outbox", estimate.Id, voyageId);

        return estimate;
    }

    public async Task<VoyageRevenueEstimate> UpdateRevenueestimateAsync(Guid id, UpdateRevenueestimateRequest request)
    {
        var estimate = await _db.VoyageRevenueEstimates.FirstOrDefaultAsync(e => e.Id == id);
        if (estimate == null)
            throw new VoyagePlanningNotFoundException($"Revenue estimate {id} not found");

        // Update only provided fields
        if (!string.IsNullOrEmpty(request.RevenueCategory))
            estimate.RevenueCategory = request.RevenueCategory;
        if (!string.IsNullOrEmpty(request.Description))
            estimate.Description = request.Description;
        if (request.EstimatedAmount.HasValue)
            estimate.EstimatedAmount = request.EstimatedAmount.Value;
        if (!string.IsNullOrEmpty(request.Currency))
            estimate.Currency = request.Currency;

        // Update sync metadata
        estimate.SyncVersion++;
        estimate.UpdatedAt = DateTime.UtcNow;
        estimate.IsSynced = false;

        await _db.SaveChangesAsync();

        await _syncOutbox.EnqueueAsync(
            targetNode: "*",
            tableName: "voyage_revenue_estimate",
            recordKey: estimate.Id.ToString(),
            action: SyncActionType.UPDATE,
            payload: estimate
        );

        _logger.LogInformation("[VOYAGE-PLANNING] Updated revenue estimate {EstimateId}, version {Version}, " +
            "enqueued to sync outbox", estimate.Id, estimate.SyncVersion);

        return estimate;
    }

    public async Task DeleteRevenueestimateAsync(Guid id)
    {
        var estimate = await _db.VoyageRevenueEstimates.FindAsync(id);
        if (estimate == null)
            throw new VoyagePlanningNotFoundException($"Revenue estimate {id} not found");

        _db.VoyageRevenueEstimates.Remove(estimate);
        await _db.SaveChangesAsync();

        await _syncOutbox.EnqueueAsync(
            targetNode: "*",
            tableName: "voyage_revenue_estimate",
            recordKey: estimate.Id.ToString(),
            action: SyncActionType.DELETE,
            payload: estimate
        );

        _logger.LogInformation("[VOYAGE-PLANNING] Deleted revenue estimate {EstimateId}, " +
            "enqueued delete to sync outbox", estimate.Id);
    }
}
