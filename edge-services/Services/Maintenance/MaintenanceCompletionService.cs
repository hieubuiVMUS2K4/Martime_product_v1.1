using MaritimeEdge.Data;
using MaritimeEdge.Models;
using MaritimeEdge.Repositories;
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

            // 3. Deduct spare parts from inventory
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

                // Check if sufficient quantity available
                if (materialItem.OnHandQuantity < usage.QuantityUsed)
                {
                    var error = $"Insufficient stock for {materialItem.Name} (Available: {materialItem.OnHandQuantity}, Required: {usage.QuantityUsed})";
                    deductionErrors.Add(error);
                    _logger.LogWarning(error);
                    
                    // Still allow completion but log warning
                }

                // Deduct quantity
                var previousQuantity = materialItem.OnHandQuantity;
                materialItem.OnHandQuantity -= usage.QuantityUsed;
                materialItem.UpdatedAt = DateTime.UtcNow;

                deductedItems.Add(new SparePartDeduction
                {
                    MaterialItemId = materialItem.Id,
                    MaterialCode = materialItem.ItemCode,
                    MaterialName = materialItem.Name,
                    QuantityUsed = usage.QuantityUsed,
                    PreviousStock = previousQuantity,
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
                    
                    // TODO: Create low stock alert notification
                }

                // Entity is already tracked - no need for explicit Update()
                // _context.MaterialItems.Update(materialItem);
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
                // Get first asset from group for running hours tracking
                var firstAsset = await _context.EquipmentGroupMembers
                    .Where(egm => egm.GroupId == schedule.EquipmentGroupId)
                    .Include(egm => egm.Asset)
                    .Select(egm => egm.Asset)
                    .FirstOrDefaultAsync();
                
                var history = new MaintenanceHistory
                {
                    ScheduleId = schedule.Id,
                    TaskId = task.Id,
                    ExecutedAt = DateTime.UtcNow,
                    ExecutedRunningHours = firstAsset != null ? await GetCurrentRunningHours(firstAsset.Id) : null,
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
                
                if (firstAsset != null)
                {
                    CalculateNextDueDate(schedule, firstAsset);
                }

                await _scheduleRepository.UpdateAsync(schedule);
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
            var currentHours = asset.CurrentRunningHours ?? 0;
            schedule.NextDueRunningHours = currentHours + schedule.IntervalHours.Value;
            
            // Estimate date (10 hours per day average)
            var hoursRemaining = schedule.NextDueRunningHours.Value - currentHours;
            var daysRemaining = (int)(hoursRemaining / 10.0);
            schedule.NextDueDate = DateTime.UtcNow.AddDays(daysRemaining);
            
            _logger.LogInformation(
                "Schedule {Code}: Next due at {Hours} running hours (estimated {Days} days from now)",
                schedule.ScheduleCode, schedule.NextDueRunningHours.Value, daysRemaining);
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
                var daysRemaining = (int)(hoursRemaining / 10.0);
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
