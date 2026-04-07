using MaritimeEdge.Data;
using MaritimeEdge.Models;
using MaritimeEdge.Repositories;
using MaritimeEdge.Constants;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace MaritimeEdge.Services.Maintenance;

/// <summary>
/// Service to handle maintenance task completion with automatic spare parts deduction
/// This is the "chức năng sống còn" - critical functionality
/// </summary>
public class MaintenanceCompletionService
{
    private readonly EdgeDbContext _context;
    private readonly IMaintenanceScheduleRepository _scheduleRepository;
    private readonly IEquipmentAssetRepository _assetRepository;
    private readonly ILogger<MaintenanceCompletionService> _logger;

    public MaintenanceCompletionService(
        EdgeDbContext context,
        IMaintenanceScheduleRepository scheduleRepository,
        IEquipmentAssetRepository assetRepository,
        ILogger<MaintenanceCompletionService> logger)
    {
        _context = context;
        _scheduleRepository = scheduleRepository;
        _assetRepository = assetRepository;
        _logger = logger;
    }

    /// <summary>
    /// Complete maintenance task with automatic spare parts inventory deduction
    /// </summary>
    public async Task<CompletionResult> CompleteTaskAsync(
        Guid taskId,
        string completedBy,
        List<SparePartUsage> sparePartsUsed,
        string? notes = null,
        string? conditionAfter = null)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // 1. Get maintenance task
            var task = await _context.MaintenanceTasks
                .FirstOrDefaultAsync(t => t.Id == taskId && !t.IsDeleted);

            if (task == null)
                return CompletionResult.Failure("Task not found");

            if (task.Status == "COMPLETED")
                return CompletionResult.Failure("Task already completed");

            // 2. Find related schedule (if task was auto-generated)
            MaintenanceSchedule? schedule = null;
            if (task.TaskId.StartsWith("SCHED-"))
            {
                var scheduleCode = ExtractScheduleCode(task.TaskId);
                schedule = await _context.MaintenanceSchedules
                    .FirstOrDefaultAsync(s => s.ScheduleCode == scheduleCode);
            }

            // 3. Deduct spare parts from InventoryStock (source of truth for ROB).
            //    Strategy A: deduct from the location with the largest available quantity first.
            //    After deduction, sync MaterialItem.OnHandQuantity = sum of remaining InventoryStock.
            var deductionErrors = new List<string>();
            var deductedItems = new List<SparePartDeduction>();

            foreach (var usage in sparePartsUsed)
            {
                var materialItem = await _context.MaterialItems
                    .FirstOrDefaultAsync(m => m.Id == usage.MaterialItemId);

                if (materialItem == null)
                {
                    deductionErrors.Add($"Material item {usage.MaterialItemId} not found");
                    continue;
                }

                // Total available = sum across all InventoryStock locations
                var stocks = await _context.InventoryStocks
                    .Where(s => s.MaterialItemId == usage.MaterialItemId)
                    .OrderByDescending(s => s.Quantity)
                    .ToListAsync();

                var totalAvailable = stocks.Any()
                    ? (double)stocks.Sum(s => s.Quantity)
                    : materialItem.OnHandQuantity; // fallback for items not yet in inventory stock

                if (totalAvailable < usage.QuantityUsed)
                {
                    var error = $"Insufficient stock for {materialItem.Name} (Available: {totalAvailable}, Required: {usage.QuantityUsed})";
                    deductionErrors.Add(error);
                    _logger.LogWarning(error);
                    // Still allow completion but log warning
                }

                var previousTotal = totalAvailable;
                var remaining = (decimal)usage.QuantityUsed;

                if (stocks.Any())
                {
                    // Deduct from InventoryStock locations (largest first)
                    foreach (var stock in stocks)
                    {
                        if (remaining <= 0) break;
                        var take = Math.Min(stock.Quantity, remaining);
                        stock.Quantity -= take;
                        stock.UpdatedAt = DateTime.UtcNow;
                        remaining -= take;
                        _logger.LogInformation(
                            "Deducted {Take} of {Code} from location {Loc}",
                            take, materialItem.ItemCode, stock.StoreLocationId);
                    }

                    // Sync MaterialItem.OnHandQuantity = sum of InventoryStock
                    var newTotal = (double)stocks.Sum(s => s.Quantity);
                    materialItem.OnHandQuantity = newTotal;
                }
                else
                {
                    // No InventoryStock records yet — fall back to deducting from MaterialItem directly
                    materialItem.OnHandQuantity -= usage.QuantityUsed;
                    _logger.LogWarning(
                        "No InventoryStock records for {Code} — deducted from MaterialItem.OnHandQuantity directly",
                        materialItem.ItemCode);
                }

                materialItem.UpdatedAt = DateTime.UtcNow;

                deductedItems.Add(new SparePartDeduction
                {
                    MaterialItemId = materialItem.Id,
                    MaterialCode = materialItem.ItemCode,
                    MaterialName = materialItem.Name,
                    QuantityUsed = usage.QuantityUsed,
                    PreviousStock = previousTotal,
                    NewStock = materialItem.OnHandQuantity,
                    UnitCost = materialItem.UnitCost
                });

                // Check if below minimum stock - create alert
                if (materialItem.MinStock.HasValue && materialItem.OnHandQuantity < materialItem.MinStock.Value)
                {
                    _logger.LogWarning(
                        "Low stock alert: {ItemCode} {ItemName} - Current: {Current}, Min: {Min}",
                        materialItem.ItemCode, materialItem.Name,
                        materialItem.OnHandQuantity, materialItem.MinStock.Value);
                }
            }

            // 4. Update task status
            task.Status = "COMPLETED";
            task.CompletedAt = DateTime.UtcNow;
            task.CompletedBy = completedBy;
            task.Notes = string.IsNullOrEmpty(task.Notes) 
                ? notes 
                : task.Notes + "\n\n" + notes;
            task.UpdatedAt = DateTime.UtcNow;

            // Store spare parts used in task
            if (sparePartsUsed.Any())
            {
                task.SparePartsUsed = JsonSerializer.Serialize(deductedItems.Select(d => new
                {
                    materialItemId = d.MaterialItemId,
                    materialCode = d.MaterialCode,
                    materialName = d.MaterialName,
                    quantityUsed = d.QuantityUsed,
                    unitCost = d.UnitCost
                }));
            }

            // Entity is already tracked - no need for explicit Update()
            // _context.MaintenanceTasks.Update(task);

            // 5. Create maintenance history record
            if (schedule != null)
            {
                // Get asset for running hours tracking
                // Per-asset schedule: use the asset directly
                // Group-based schedule: use first asset from group
                EquipmentAsset? trackingAsset = null;
                if (schedule.EquipmentAssetId.HasValue)
                {
                    trackingAsset = await _context.EquipmentAssets
                        .FirstOrDefaultAsync(a => a.Id == schedule.EquipmentAssetId.Value);
                }
                else if (schedule.EquipmentGroupId.HasValue)
                {
                    trackingAsset = await _context.EquipmentGroupMembers
                        .Where(egm => egm.GroupId == schedule.EquipmentGroupId.Value)
                        .Include(egm => egm.Asset)
                        .Select(egm => egm.Asset)
                        .FirstOrDefaultAsync();
                }
                
                var history = new MaintenanceHistory
                {
                    ScheduleId = schedule.Id,
                    TaskId = task.Id,
                    ExecutedAt = DateTime.UtcNow,
                    ExecutedRunningHours = trackingAsset != null ? await GetCurrentRunningHours(trackingAsset.Id) : null,
                    CompletedBy = completedBy,
                    ActualDurationHours = task.StartedAt.HasValue 
                        ? (DateTime.UtcNow - task.StartedAt.Value).TotalHours 
                        : null,
                    SparePartsUsed = JsonSerializer.Serialize(deductedItems),
                    TotalSparePartsCost = deductedItems.Sum(d => d.UnitCost * (decimal)d.QuantityUsed),
                    Notes = notes,
                    ConditionAfter = conditionAfter,
                    CreatedAt = DateTime.UtcNow
                };

                _context.MaintenanceHistories.Add(history);

                // 6. Update schedule - set last execution and calculate next due
                schedule.LastExecutedAt = DateTime.UtcNow;
                schedule.LastExecutedRunningHours = history.ExecutedRunningHours;
                
                if (trackingAsset != null)
                {
                    CalculateNextDueDate(schedule, trackingAsset);
                }

                await _scheduleRepository.UpdateAsync(schedule);

                // 7. RECURRENCE (Gối đầu): Generate next cycle task for PERIODIC schedules
                if (schedule.MaintenanceCategory == "PERIODIC" && schedule.IsActive && schedule.AutoGenerate)
                {
                    await GenerateNextCycleTask(schedule, task, trackingAsset);
                }
            }

            // 7. Save all changes
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation(
                "Task {TaskId} completed by {CompletedBy}. Deducted {Count} spare parts.",
                task.TaskId, completedBy, deductedItems.Count);

            return CompletionResult.Success(
                deductedItems,
                deductionErrors.Any() ? deductionErrors : null);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error completing task {TaskId}", taskId);
            return CompletionResult.Failure($"Error: {ex.Message}");
        }
    }

    private async Task<double?> GetCurrentRunningHours(Guid assetId)
    {
        var asset = await _assetRepository.GetByIdAsync(assetId);
        return asset?.CurrentRunningHours;
    }

    private string ExtractScheduleCode(string taskId)
    {
        // Format: SCHED-{ScheduleCode}-{Date}
        // Example: SCHED-ME-OIL-CHANGE-20251207
        var parts = taskId.Split('-');
        if (parts.Length >= 3)
        {
            // Remove "SCHED" prefix and date suffix
            return string.Join("-", parts.Skip(1).Take(parts.Length - 2));
        }
        return string.Empty;
    }

    /// <summary>
    /// RECURRENCE (Gối đầu): After a PERIODIC task is completed,
    /// automatically generate the next cycle task as SCHEDULED.
    /// NextDueRH = LastCompletedRH + IntervalHours (using actual RH at completion for accuracy)
    /// </summary>
    private async Task GenerateNextCycleTask(MaintenanceSchedule schedule, MaintenanceTask completedTask, EquipmentAsset? trackingAsset)
    {
        try
        {
            if (!schedule.NextDueDate.HasValue)
            {
                _logger.LogWarning("Schedule {Code}: Cannot generate next cycle task — no NextDueDate calculated", schedule.ScheduleCode);
                return;
            }

            // Build new TaskId with next due date
            var equipmentCode = completedTask.EquipmentId ?? completedTask.EquipmentAssetName ?? "GRP";
            // Extract equipment identifier from completed task's TaskId (format: SCHED-{code}-{equip}-{date})
            var parts = completedTask.TaskId.Split('-');
            if (parts.Length >= 4)
            {
                // Take the part before the date suffix
                equipmentCode = parts[^2]; // Second to last = equipment code
            }

            var nextTaskId = $"SCHED-{schedule.ScheduleCode}-{equipmentCode}-{schedule.NextDueDate.Value:yyyyMMdd}";

            // Check for duplicates
            if (await _context.MaintenanceTasks.AnyAsync(t => t.TaskId == nextTaskId && !t.IsDeleted))
            {
                _logger.LogDebug("Next cycle task {TaskId} already exists, skipping", nextTaskId);
                return;
            }

            // Build spare parts JSON from schedule
            string? sparePartsJson = null;
            var spareParts = await _context.ScheduleSpareParts
                .Where(sp => sp.ScheduleId == schedule.Id)
                .ToListAsync();

            if (spareParts.Any())
            {
                var materialIds = spareParts.Select(sp => sp.MaterialItemId).ToList();
                var materials = await _context.MaterialItems
                    .Where(m => materialIds.Contains(m.Id))
                    .ToDictionaryAsync(m => m.Id, m => new { m.ItemCode, m.Name });

                sparePartsJson = JsonSerializer.Serialize(
                    spareParts.Select(sp => new
                    {
                        materialItemId = sp.MaterialItemId,
                        materialCode = materials.ContainsKey(sp.MaterialItemId) ? materials[sp.MaterialItemId].ItemCode : "",
                        materialName = materials.ContainsKey(sp.MaterialItemId) ? materials[sp.MaterialItemId].Name : "Unknown",
                        quantityRequired = sp.QuantityRequired,
                        isMandatory = sp.IsMandatory
                    }),
                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            }

            var nextTask = new MaintenanceTask
            {
                TaskId = nextTaskId,
                ScheduleId = schedule.Id,
                EquipmentGroupId = completedTask.EquipmentGroupId,
                EquipmentGroupName = completedTask.EquipmentGroupName,
                EquipmentAssetId = completedTask.EquipmentAssetId,
                EquipmentAssetName = completedTask.EquipmentAssetName,
                EquipmentId = completedTask.EquipmentId,
                EquipmentName = completedTask.EquipmentName,
                TaskType = completedTask.TaskType,
                TaskDescription = completedTask.TaskDescription,
                IntervalHours = schedule.IntervalHours,
                IntervalDays = schedule.IntervalDays,
                LastDoneAt = schedule.LastExecutedAt,
                NextDueAt = schedule.NextDueDate!.Value,
                RunningHoursAtLastDone = schedule.LastExecutedRunningHours,
                Priority = schedule.Priority ?? completedTask.Priority,
                Status = "SCHEDULED",
                EstimatedDuration = completedTask.EstimatedDuration,
                RequiredSpareParts = sparePartsJson,
                AssignedTo = completedTask.AssignedTo,
                Notes = $"Auto-generated: next cycle after {completedTask.TaskId}",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.MaintenanceTasks.Add(nextTask);

            // Copy checklist templates for the new task
            var checklistTemplates = await _context.ScheduleChecklistTemplates
                .Where(t => t.ScheduleId == schedule.Id)
                .OrderBy(t => t.SequenceOrder)
                .ToListAsync();

            if (checklistTemplates.Any())
            {
                // Get assets for checklist
                List<EquipmentAsset> assets;
                if (completedTask.EquipmentAssetId.HasValue)
                {
                    var asset = await _context.EquipmentAssets.FindAsync(completedTask.EquipmentAssetId.Value);
                    assets = asset != null ? new List<EquipmentAsset> { asset } : new List<EquipmentAsset>();
                }
                else if (completedTask.EquipmentGroupId.HasValue)
                {
                    assets = await _context.EquipmentGroupMembers
                        .Where(egm => egm.GroupId == completedTask.EquipmentGroupId.Value)
                        .Include(egm => egm.Asset)
                        .Select(egm => egm.Asset)
                        .Where(a => a != null && a.IsActive)
                        .Cast<EquipmentAsset>()
                        .ToListAsync();
                }
                else
                {
                    assets = new List<EquipmentAsset>();
                }

                foreach (var asset in assets)
                {
                    foreach (var template in checklistTemplates)
                    {
                        _context.TaskChecklistItems.Add(new TaskChecklistItem
                        {
                            TaskId = nextTask.TaskId,
                            AssetId = asset.Id,
                            AssetCode = asset.AssetCode,
                            AssetName = asset.AssetName,
                            SequenceOrder = template.SequenceOrder,
                            CheckpointDescription = template.CheckpointDescription,
                            RequiresReading = template.RequiresReading,
                            NormalRangeMin = template.NormalRangeMin,
                            NormalRangeMax = template.NormalRangeMax,
                            Unit = template.Unit,
                            IsCompleted = false,
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }
            }

            _logger.LogInformation(
                "RECURRENCE: Generated next cycle task {NextTaskId} (NextDueDate={NextDue}, NextDueRH={NextDueRH}) after completing {CompletedTaskId}",
                nextTaskId, schedule.NextDueDate?.ToString("yyyy-MM-dd"),
                schedule.NextDueRunningHours, completedTask.TaskId);
        }
        catch (Exception ex)
        {
            // Don't fail the completion if next cycle generation fails
            _logger.LogError(ex, "Error generating next cycle task for schedule {Code} after completing {TaskId}",
                schedule.ScheduleCode, completedTask.TaskId);
        }
    }

    private void CalculateNextDueDate(MaintenanceSchedule schedule, EquipmentAsset asset)
    {
        if (schedule.IntervalType == "CALENDAR" && schedule.IntervalDays.HasValue)
        {
            // Use LastExecutedAt (completion date) as base for next interval
            // This ensures interval consistency regardless of late completion
            var baseDate = schedule.LastExecutedAt ?? DateTime.UtcNow;
            schedule.NextDueDate = baseDate.AddDays(schedule.IntervalDays.Value);
            
            _logger.LogInformation(
                "Schedule {Code}: Next due calculated from {Base} + {Interval} days = {NextDue}",
                schedule.ScheduleCode, baseDate.ToString("yyyy-MM-dd"), 
                schedule.IntervalDays.Value, schedule.NextDueDate?.ToString("yyyy-MM-dd"));
        }
        else if (schedule.IntervalType == "RUNNING_HOURS" && schedule.IntervalHours.HasValue)
        {
            // Formula: NextDueRH = LastCompletedRH + IntervalHours
            // Uses actual running hours at completion time (more accurate than theoretical)
            var completedAtRH = schedule.LastExecutedRunningHours ?? (asset.CurrentRunningHours ?? 0);
            schedule.NextDueRunningHours = completedAtRH + schedule.IntervalHours.Value;
            
            // Estimate date (10 hours per day average)
            var currentHours = asset.CurrentRunningHours ?? 0;
            var hoursRemaining = schedule.NextDueRunningHours.Value - currentHours;
            var daysRemaining = (int)Math.Max(hoursRemaining / MaintenanceConstants.AVERAGE_HOURS_PER_DAY, 1);
            schedule.NextDueDate = DateTime.UtcNow.AddDays(daysRemaining);
            
            _logger.LogInformation(
                "Schedule {Code}: Next due at {Hours} RH (completedAt={CompletedRH} + interval={Interval}), estimated {Days} days",
                schedule.ScheduleCode, schedule.NextDueRunningHours.Value, 
                completedAtRH, schedule.IntervalHours.Value, daysRemaining);
        }
        else if (schedule.IntervalType == "HYBRID")
        {
            DateTime? calendarDue = null;
            DateTime? runningHoursDue = null;

            if (schedule.IntervalDays.HasValue)
            {
                var baseDate = schedule.LastExecutedAt ?? DateTime.UtcNow;
                calendarDue = baseDate.AddDays(schedule.IntervalDays.Value);
            }

            if (schedule.IntervalHours.HasValue)
            {
                var currentHours = asset.CurrentRunningHours ?? 0;
                schedule.NextDueRunningHours = currentHours + schedule.IntervalHours.Value;
                
                var hoursRemaining = schedule.NextDueRunningHours.Value - currentHours;
                var daysRemaining = (int)Math.Max(hoursRemaining / MaintenanceConstants.AVERAGE_HOURS_PER_DAY, 1);
                runningHoursDue = DateTime.UtcNow.AddDays(daysRemaining);
            }

            schedule.NextDueDate = (calendarDue.HasValue && runningHoursDue.HasValue)
                ? (calendarDue < runningHoursDue ? calendarDue : runningHoursDue)
                : (calendarDue ?? runningHoursDue);
        }
    }
}

// DTOs for completion service
public class SparePartUsage
{
    public Guid MaterialItemId { get; set; }
    public double QuantityUsed { get; set; }
}

public class SparePartDeduction
{
    public Guid MaterialItemId { get; set; }
    public string MaterialCode { get; set; } = string.Empty;
    public string MaterialName { get; set; } = string.Empty;
    public double QuantityUsed { get; set; }
    public double PreviousStock { get; set; }
    public double NewStock { get; set; }
    public decimal? UnitCost { get; set; }
}

public class CompletionResult
{
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public List<SparePartDeduction>? DeductedItems { get; set; }
    public List<string>? Warnings { get; set; }

    public static CompletionResult Success(
        List<SparePartDeduction> deductedItems, 
        List<string>? warnings = null)
    {
        return new CompletionResult
        {
            IsSuccess = true,
            DeductedItems = deductedItems,
            Warnings = warnings
        };
    }

    public static CompletionResult Failure(string errorMessage)
    {
        return new CompletionResult
        {
            IsSuccess = false,
            ErrorMessage = errorMessage
        };
    }
}
