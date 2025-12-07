using MaritimeEdge.Data;
using MaritimeEdge.Models;
using MaritimeEdge.Repositories;
using Microsoft.EntityFrameworkCore;

namespace MaritimeEdge.Services;

/// <summary>
/// Background service to automatically generate maintenance tasks from schedules
/// Runs daily to check schedules and create tasks X days before due date
/// </summary>
public class MaintenanceSchedulerService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MaintenanceSchedulerService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(6); // Check every 6 hours

    public MaintenanceSchedulerService(
        IServiceProvider serviceProvider,
        ILogger<MaintenanceSchedulerService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <summary>
    /// Get minimum lead time based on priority (ISM Code & IACS guidelines)
    /// </summary>
    private int GetMinimumLeadTime(string priority)
    {
        return priority switch
        {
            "CRITICAL" => 30,  // 30 days for critical equipment
            "HIGH" => 14,      // 14 days for high priority
            "MEDIUM" => 10,    // 10 days for medium priority
            "LOW" => 7,        // 7 days minimum
            _ => 7
        };
    }

    /// <summary>
    /// Calculate optimal lead time considering work duration and spare parts
    /// </summary>
    private int CalculateRecommendedLeadTime(MaintenanceSchedule schedule)
    {
        // Base: 3x estimated duration (safety buffer for delays)
        var workDays = (int)Math.Ceiling((schedule.EstimatedDurationHours ?? 4) / 8.0);
        var recommendedDays = workDays * 3;
        
        // Apply priority-based minimum
        var priorityMinimum = GetMinimumLeadTime(schedule.Priority);
        
        return Math.Max(recommendedDays, priorityMinimum);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Maintenance Scheduler Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await GenerateTasksFromSchedules();
                await Task.Delay(_checkInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Maintenance Scheduler Service is stopping");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Maintenance Scheduler Service");
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken); // Wait 5 min on error
            }
        }
    }

    private async Task GenerateTasksFromSchedules()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<EdgeDbContext>();
        var scheduleRepo = scope.ServiceProvider.GetRequiredService<IMaintenanceScheduleRepository>();
        var assetRepo = scope.ServiceProvider.GetRequiredService<IEquipmentAssetRepository>();

        try
        {
            _logger.LogInformation("Checking schedules for auto task generation...");

            var now = DateTime.UtcNow;
            var schedules = await scheduleRepo.GetAutoGenerateSchedulesAsync();

            int tasksGenerated = 0;

            foreach (var schedule in schedules)
            {
                if (!schedule.NextDueDate.HasValue)
                {
                    // Calculate next due date if not set
                    // Get first active asset from the group (for interval calculation)
                    var groupMembers = await context.EquipmentGroupMembers
                        .Where(egm => egm.GroupId == schedule.EquipmentGroupId)
                        .Include(egm => egm.Asset)
                        .ToListAsync();
                    
                    var asset = groupMembers
                        .Select(gm => gm.Asset)
                        .Where(a => a != null && a.IsActive)
                        .OrderByDescending(a => a.CurrentRunningHours ?? 0)
                        .FirstOrDefault();
                    
                    if (asset != null)
                    {
                        CalculateNextDueDate(schedule, asset);
                        await scheduleRepo.UpdateAsync(schedule);
                    }
                    continue;
                }

                // Check if task should be generated (X days before due)
                var daysUntilDue = (schedule.NextDueDate.Value.Date - now.Date).Days;
                
                // SAFETY CHECK: Validate lead time is sufficient
                var minimumLeadTime = GetMinimumLeadTime(schedule.Priority);
                var effectiveLeadTime = Math.Max(schedule.DaysBeforeDue, minimumLeadTime);
                
                if (effectiveLeadTime > schedule.DaysBeforeDue)
                {
                    _logger.LogWarning(
                        "Schedule {ScheduleCode} has insufficient lead time ({Configured} days). " +
                        "Using minimum {Minimum} days for {Priority} priority. " +
                        "Recommended: {Recommended} days for {Hours}h task.",
                        schedule.ScheduleCode, 
                        schedule.DaysBeforeDue,
                        minimumLeadTime,
                        schedule.Priority,
                        CalculateRecommendedLeadTime(schedule),
                        schedule.EstimatedDurationHours ?? 0);
                }
                
                if (daysUntilDue <= effectiveLeadTime)
                {
                    // Check if task already exists for this schedule and due date
                    var existingTask = await context.MaintenanceTasks
                        .Where(t => t.TaskId.StartsWith($"SCHED-{schedule.ScheduleCode}") &&
                                   t.Status != "COMPLETED" &&
                                   t.NextDueAt.Date == schedule.NextDueDate.Value.Date)
                        .FirstOrDefaultAsync();

                    if (existingTask == null)
                    {
                        await GenerateTaskFromSchedule(context, schedule);
                        tasksGenerated++;
                    }
                }
            }

            if (tasksGenerated > 0)
            {
                _logger.LogInformation("Generated {Count} maintenance tasks from schedules", tasksGenerated);
            }
            else
            {
                _logger.LogDebug("No new tasks to generate");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating tasks from schedules");
        }
    }

    private async Task GenerateTaskFromSchedule(EdgeDbContext context, MaintenanceSchedule schedule)
    {
        try
        {
            // Get equipment group
            var group = await context.EquipmentGroups.FindAsync(schedule.EquipmentGroupId);
            if (group == null)
            {
                _logger.LogWarning("Equipment group not found for schedule {ScheduleCode}", schedule.ScheduleCode);
                return;
            }

            // Get all assets in the group
            var groupMembers = await context.EquipmentGroupMembers
                .Where(egm => egm.GroupId == schedule.EquipmentGroupId)
                .Include(egm => egm.Asset)
                .ToListAsync();

            if (!groupMembers.Any())
            {
                _logger.LogWarning("No assets found in group {GroupCode} for schedule {ScheduleCode}", 
                    group.GroupCode, schedule.ScheduleCode);
                return;
            }

            // Get task type
            var taskType = await context.TaskTypes.FindAsync(schedule.TaskTypeId);
            if (taskType == null)
            {
                _logger.LogWarning("TaskType not found for schedule {ScheduleCode}", schedule.ScheduleCode);
                return;
            }

            // Get spare parts requirements
            var spareParts = await context.ScheduleSpareParts
                .Where(sp => sp.ScheduleId == schedule.Id)
                .ToListAsync();

            // Build spare parts JSON
            var sparePartsJson = spareParts.Any()
                ? System.Text.Json.JsonSerializer.Serialize(spareParts.Select(sp => new
                {
                    materialItemId = sp.MaterialItemId,
                    quantityRequired = sp.QuantityRequired,
                    isMandatory = sp.IsMandatory
                }))
                : null;

            // Generate task for EACH asset in the group
            int tasksCreated = 0;
            foreach (var member in groupMembers)
            {
                var asset = member.Asset;
                if (asset == null || !asset.IsActive) continue;

                // Create unique task ID with asset code
                var taskId = $"SCHED-{schedule.ScheduleCode}-{asset.AssetCode}-{DateTime.UtcNow:yyyyMMdd}";

                // Check if task already exists
                var existingTask = await context.MaintenanceTasks
                    .FirstOrDefaultAsync(t => t.TaskId == taskId && t.Status != "COMPLETED");
                
                if (existingTask != null)
                {
                    _logger.LogDebug("Task {TaskId} already exists, skipping", taskId);
                    continue;
                }

                // Create maintenance task for this asset
                var task = new MaintenanceTask
                {
                    TaskId = taskId,
                    TaskTypeId = schedule.TaskTypeId,
                    EquipmentId = asset.AssetCode,
                    EquipmentName = asset.AssetName,
                    TaskType = schedule.IntervalType,
                    TaskDescription = schedule.ScheduleName + "\n\n" + (schedule.Instructions ?? ""),
                    IntervalHours = schedule.IntervalHours,
                    IntervalDays = schedule.IntervalDays,
                    LastDoneAt = schedule.LastExecutedAt,
                    NextDueAt = schedule.NextDueDate!.Value,
                    RunningHoursAtLastDone = schedule.LastExecutedRunningHours,
                    Priority = schedule.Priority,
                    Status = "PENDING",
                    SparePartsUsed = sparePartsJson,
                    Notes = $"Auto-generated from schedule: {schedule.ScheduleCode} (Group: {group.GroupName})",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                context.MaintenanceTasks.Add(task);
                tasksCreated++;

                // Create task details (checklist items) from TaskType
                var taskDetails = await context.TaskDetails
                    .Where(td => td.TaskTypes.Any(tt => tt.Id == schedule.TaskTypeId))
                    .OrderBy(td => td.OrderIndex)
                    .ToListAsync();

                foreach (var detail in taskDetails)
                {
                    var taskDetail = new MaintenanceTaskDetail
                    {
                        MaintenanceTaskId = task.Id,
                        TaskDetailId = detail.Id,
                        Status = "PENDING",
                        IsCompleted = false,
                        CreatedAt = DateTime.UtcNow
                    };
                    context.MaintenanceTaskDetails.Add(taskDetail);
                }
            }

            if (tasksCreated > 0)
            {
                await context.SaveChangesAsync();
                _logger.LogInformation(
                    "Generated {Count} task(s) for schedule {ScheduleCode} (Group: {GroupName})",
                    tasksCreated, schedule.ScheduleCode, group.GroupName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating task from schedule {ScheduleCode}", schedule.ScheduleCode);
        }
    }

    private void CalculateNextDueDate(MaintenanceSchedule schedule, EquipmentAsset asset)
    {
        if (schedule.IntervalType == "CALENDAR" && schedule.IntervalDays.HasValue)
        {
            var baseDate = schedule.LastExecutedAt ?? DateTime.UtcNow;
            schedule.NextDueDate = baseDate.AddDays(schedule.IntervalDays.Value);
        }
        else if (schedule.IntervalType == "RUNNING_HOURS" && schedule.IntervalHours.HasValue)
        {
            var baseHours = schedule.LastExecutedRunningHours ?? asset.CurrentRunningHours ?? 0;
            schedule.NextDueRunningHours = baseHours + schedule.IntervalHours.Value;
            
            // Estimate calendar date based on average 10 hours per day
            var hoursRemaining = schedule.NextDueRunningHours.Value - (asset.CurrentRunningHours ?? 0);
            var daysRemaining = (int)(hoursRemaining / 10.0);
            schedule.NextDueDate = DateTime.UtcNow.AddDays(daysRemaining);
        }
        else if (schedule.IntervalType == "HYBRID")
        {
            // Use the earlier of the two due dates
            DateTime? calendarDue = null;
            DateTime? runningHoursDue = null;

            if (schedule.IntervalDays.HasValue)
            {
                var baseDate = schedule.LastExecutedAt ?? DateTime.UtcNow;
                calendarDue = baseDate.AddDays(schedule.IntervalDays.Value);
            }

            if (schedule.IntervalHours.HasValue)
            {
                var baseHours = schedule.LastExecutedRunningHours ?? asset.CurrentRunningHours ?? 0;
                schedule.NextDueRunningHours = baseHours + schedule.IntervalHours.Value;
                
                var hoursRemaining = schedule.NextDueRunningHours.Value - (asset.CurrentRunningHours ?? 0);
                var daysRemaining = (int)(hoursRemaining / 10.0);
                runningHoursDue = DateTime.UtcNow.AddDays(daysRemaining);
            }

            // Choose earlier date
            if (calendarDue.HasValue && runningHoursDue.HasValue)
            {
                schedule.NextDueDate = calendarDue < runningHoursDue ? calendarDue : runningHoursDue;
            }
            else
            {
                schedule.NextDueDate = calendarDue ?? runningHoursDue;
            }
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Maintenance Scheduler Service is stopping");
        return base.StopAsync(cancellationToken);
    }
}
