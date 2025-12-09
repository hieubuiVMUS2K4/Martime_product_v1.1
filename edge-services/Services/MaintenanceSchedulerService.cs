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

            // === NEW APPROACH: Create ONE task for the entire equipment group ===
            // Create unique task ID with group code
            var taskId = $"SCHED-{schedule.ScheduleCode}-{group.GroupCode}-{DateTime.UtcNow:yyyyMMdd}";

            // Check if task already exists
            var existingTask = await context.MaintenanceTasks
                .FirstOrDefaultAsync(t => t.TaskId == taskId && t.Status != "COMPLETED");
            
            if (existingTask != null)
            {
                _logger.LogDebug("Task {TaskId} already exists, skipping", taskId);
                return;
            }

            // Determine task assignee (PIC) for entire group using department-aware 4-tier waterfall
            var assignedTo = await DetermineGroupPIC(context, schedule, group);

            // Check if checklist templates exist (for validation)
            var hasChecklistTemplates = await context.ScheduleChecklistTemplates
                .AnyAsync(t => t.ScheduleId == schedule.Id);

            // Determine initial status with validation:
            // 1. Validate checklist existence
            // 2. Validate PIC assignment
            // 3. Only move to PENDING/PENDING_APPROVAL if both are satisfied
            string initialStatus = DetermineTaskStatus(assignedTo, hasChecklistTemplates, schedule.Priority);
            
            _logger.LogDebug(
                "Task status determined: {Status} (HasPIC: {HasPIC}, HasChecklist: {HasChecklist}, Priority: {Priority})",
                initialStatus, !string.IsNullOrWhiteSpace(assignedTo), hasChecklistTemplates, schedule.Priority);

            // Create maintenance task for entire group
            var task = new MaintenanceTask
            {
                TaskId = taskId,
                TaskTypeId = schedule.TaskTypeId,
                
                // NEW: Group-based fields
                EquipmentGroupId = group.Id,
                EquipmentGroupName = group.GroupName,
                
                // LEGACY: Keep null for group-based tasks (backward compatibility)
                EquipmentId = null,
                EquipmentName = null,
                
                TaskType = schedule.IntervalType,
                TaskDescription = schedule.ScheduleName + "\n\n" + (schedule.Instructions ?? ""),
                IntervalHours = schedule.IntervalHours,
                IntervalDays = schedule.IntervalDays,
                LastDoneAt = schedule.LastExecutedAt,
                NextDueAt = schedule.NextDueDate!.Value,
                RunningHoursAtLastDone = schedule.LastExecutedRunningHours,
                Priority = schedule.Priority,
                Status = initialStatus, // TASK, PENDING, or PENDING_APPROVAL based on assignment and priority
                AssignedTo = assignedTo, // Auto-assigned based on waterfall logic (null if unassigned)
                SparePartsUsed = sparePartsJson,
                Notes = $"Auto-generated from schedule: {schedule.ScheduleCode} (Group: {group.GroupName})",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            context.MaintenanceTasks.Add(task);

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

            // Create checklist items for each asset in group
            // CLONE from schedule_checklist_templates if available
            var checklistTemplates = await context.ScheduleChecklistTemplates
                .Where(t => t.ScheduleId == schedule.Id)
                .OrderBy(t => t.SequenceOrder)
                .ToListAsync();

            if (checklistTemplates.Any())
            {
                // Use templates: Create checklist items for each asset based on templates
                foreach (var member in groupMembers)
                {
                    var asset = member.Asset;
                    if (asset == null || !asset.IsActive) continue;

                    foreach (var template in checklistTemplates)
                    {
                        // Clone template values
                        var checklistItem = new TaskChecklistItem
                        {
                            TaskId = task.TaskId,
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
                        };

                        // ASSET-SPECIFIC OVERRIDE: Check if asset has custom technical specs
                        if (!string.IsNullOrWhiteSpace(asset.TechnicalSpecs))
                        {
                            try
                            {
                                var specs = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(asset.TechnicalSpecs);
                                if (specs != null)
                                {
                                    // Override ranges if asset has specific values
                                    // Format: { "oilLevel_min": 80, "oilLevel_max": 100, "temperature_min": 70, "temperature_max": 90 }
                                    var checkpointKey = template.CheckpointDescription
                                        .ToLower()
                                        .Replace(" ", "_")
                                        .Replace("check_", "")
                                        .Replace("measure_", "")
                                        .Replace("inspect_", "");

                                    if (specs.TryGetValue($"{checkpointKey}_min", out var minVal))
                                    {
                                        checklistItem.NormalRangeMin = Convert.ToDouble(minVal);
                                    }
                                    if (specs.TryGetValue($"{checkpointKey}_max", out var maxVal))
                                    {
                                        checklistItem.NormalRangeMax = Convert.ToDouble(maxVal);
                                    }
                                    if (specs.TryGetValue($"{checkpointKey}_unit", out var unitVal))
                                    {
                                        checklistItem.Unit = unitVal.ToString();
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "Failed to parse TechnicalSpecs for asset {AssetCode}", asset.AssetCode);
                            }
                        }

                        context.TaskChecklistItems.Add(checklistItem);
                    }
                }
                
                _logger.LogInformation(
                    "Cloned {TemplateCount} checklist templates × {AssetCount} assets = {TotalItems} checklist items for task {TaskId}",
                    checklistTemplates.Count, 
                    groupMembers.Count(m => m.Asset != null && m.Asset.IsActive),
                    checklistTemplates.Count * groupMembers.Count(m => m.Asset != null && m.Asset.IsActive),
                    taskId);
            }
            else
            {
                // No templates: Create simple checklist items (backward compatibility)
                foreach (var member in groupMembers)
                {
                    var asset = member.Asset;
                    if (asset == null || !asset.IsActive) continue;

                    var checklistItem = new TaskChecklistItem
                    {
                        TaskId = task.TaskId,
                        AssetId = asset.Id,
                        AssetCode = asset.AssetCode,
                        AssetName = asset.AssetName,
                        SequenceOrder = member.SequenceOrder,
                        IsCompleted = false,
                        CreatedAt = DateTime.UtcNow
                    };
                    context.TaskChecklistItems.Add(checklistItem);
                }
            }

            await context.SaveChangesAsync();
            
            var assetCount = groupMembers.Count(m => m.Asset != null && m.Asset.IsActive);
            _logger.LogInformation(
                "Generated group task {TaskId} for schedule {ScheduleCode} (Group: {GroupName}, {AssetCount} assets)",
                taskId, schedule.ScheduleCode, group.GroupName, assetCount);
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

    /// <summary>
    /// Determine task status based on validation rules:
    /// - MISSING_BOTH: No PIC and no checklist
    /// - MISSING_PIC: Has checklist but no PIC assigned
    /// - MISSING_CHECKLIST: Has PIC but no checklist defined
    /// - PENDING_APPROVAL: HIGH/CRITICAL with both PIC and checklist
    /// - PENDING: LOW/MEDIUM with both PIC and checklist
    /// </summary>
    private string DetermineTaskStatus(string? assignedTo, bool hasChecklist, string priority)
    {
        bool hasPIC = !string.IsNullOrWhiteSpace(assignedTo);

        // Validation: Check for missing requirements
        if (!hasPIC && !hasChecklist)
        {
            _logger.LogWarning("Task missing both PIC and checklist → MISSING_BOTH");
            return "MISSING_BOTH";
        }
        
        if (!hasPIC)
        {
            _logger.LogWarning("Task missing PIC assignment → MISSING_PIC");
            return "MISSING_PIC";
        }
        
        if (!hasChecklist)
        {
            _logger.LogWarning("Task missing checklist templates → MISSING_CHECKLIST");
            return "MISSING_CHECKLIST";
        }

        // Both PIC and checklist exist - determine based on priority
        if (priority == "HIGH" || priority == "CRITICAL")
        {
            _logger.LogDebug("Task ready for approval: HIGH/CRITICAL priority → PENDING_APPROVAL");
            return "PENDING_APPROVAL";
        }
        else
        {
            _logger.LogDebug("Task ready for execution: LOW/MEDIUM priority → PENDING");
            return "PENDING";
        }
    }

    /// <summary>
    /// Determine PIC (Person In Charge) for entire equipment group using 4-tier waterfall logic:
    /// 1. Group.PicCrewId (highest priority - specific person override)
    /// 2. Group.PicRole (match rank to onboard crew in group's department)
    /// 3. Schedule.DefaultAssignedTo (fallback to schedule default)
    /// 4. null (unassigned - Work Planner will assign manually → TASK status)
    /// </summary>
    private async Task<string?> DetermineGroupPIC(
        EdgeDbContext context,
        MaintenanceSchedule schedule,
        EquipmentGroup group)
    {
        try
        {
            // Get crew filtered by department (strict organizational boundary)
            IQueryable<CrewMember> departmentCrewQuery = context.CrewMembers
                .Where(c => c.IsOnboard);

            // Apply department filter if group has department assigned
            if (!string.IsNullOrWhiteSpace(group.Department))
            {
                departmentCrewQuery = departmentCrewQuery.Where(c => c.Department == group.Department);
                _logger.LogDebug("Filtering crew by department: {Department}", group.Department);
            }

            // Tier 1: PIC crew override from equipment group (e.g., 2/E assigned to Main Engine Group)
            if (!string.IsNullOrWhiteSpace(group.PicCrewId))
            {
                var picCrew = await departmentCrewQuery
                    .FirstOrDefaultAsync(c => c.CrewId == group.PicCrewId);
                
                if (picCrew != null)
                {
                    _logger.LogDebug("Group task assigned to PIC crew {CrewId} ({Rank}) via Group override", 
                        picCrew.CrewId, picCrew.Rank);
                    return picCrew.CrewId;
                }
            }

            // Tier 2: PIC role from equipment group (e.g., "2/E" for Engine Room equipment)
            if (!string.IsNullOrWhiteSpace(group.PicRole))
            {
                var picCrew = await departmentCrewQuery
                    .Where(c => c.Rank == group.PicRole)
                    .OrderBy(c => c.CrewId) // Stable ordering
                    .FirstOrDefaultAsync();
                
                if (picCrew != null)
                {
                    _logger.LogDebug("Group task assigned to PIC role {Rank} ({CrewId}) via Group PIC", 
                        group.PicRole, picCrew.CrewId);
                    return picCrew.CrewId;
                }
            }

            // Tier 3: Schedule default assignee (fallback)
            if (!string.IsNullOrWhiteSpace(schedule.AssignedToCrewId))
            {
                var defaultCrew = await context.CrewMembers
                    .FirstOrDefaultAsync(c => c.IsOnboard && c.CrewId == schedule.AssignedToCrewId);
                
                if (defaultCrew != null)
                {
                    _logger.LogDebug("Group task assigned to schedule default {CrewId}", schedule.AssignedToCrewId);
                    return defaultCrew.CrewId;
                }
            }

            // Tier 4: No assignment found - return null (Work Planner must assign → TASK status)
            _logger.LogDebug("No PIC found for group {GroupName} - task will be TASK status", group.GroupName);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error determining PIC for group {GroupName}", group.GroupName);
            return null;
        }
    }

    /// <summary>
    /// LEGACY METHOD - Kept for backward compatibility with old per-asset tasks
    /// Determine task assignee using department-aware 5-tier waterfall logic:
    /// 0. Filter crew by Equipment Group's Department (strict boundary)
    /// 1. Group.PicCrewId (highest priority - equipment PIC override)
    /// 2. Group.PicRole (Person In Charge rank for this equipment)
    /// 3. Schedule.AssignedToCrewId (specific crew override)
    /// 4. Schedule.AssignedToRole (match role to onboard crew)
    /// 5. Asset.DefaultExecutorRole (equipment-based default)
    /// 6. null (unassigned - Work Planner will assign manually)
    /// </summary>
    private async Task<string?> DetermineTaskAssignee(
        EdgeDbContext context,
        MaintenanceSchedule schedule,
        EquipmentAsset asset,
        EquipmentGroup group)
    {
        try
        {
            // Get crew filtered by department (strict organizational boundary)
            IQueryable<CrewMember> departmentCrewQuery = context.CrewMembers
                .Where(c => c.IsOnboard);

            // Apply department filter if group has department assigned
            if (!string.IsNullOrWhiteSpace(group.Department))
            {
                departmentCrewQuery = departmentCrewQuery.Where(c => c.Department == group.Department);
                _logger.LogDebug("Filtering crew by department: {Department}", group.Department);
            }

            // Tier 1: PIC crew override from equipment group (e.g., 2/E assigned to Main Engine)
            if (!string.IsNullOrWhiteSpace(group.PicCrewId))
            {
                var picCrew = await departmentCrewQuery
                    .FirstOrDefaultAsync(c => c.CrewId == group.PicCrewId);
                
                if (picCrew != null)
                {
                    _logger.LogDebug("Task assigned to PIC crew {CrewId} ({Rank}) via Group override", 
                        picCrew.CrewId, picCrew.Rank);
                    return picCrew.CrewId;
                }
            }

            // Tier 2: PIC role from equipment group (e.g., "2/E" for Main Engine)
            if (!string.IsNullOrWhiteSpace(group.PicRole))
            {
                var picCrew = await departmentCrewQuery
                    .Where(c => c.Rank == group.PicRole)
                    .OrderBy(c => c.CrewId) // Stable ordering
                    .FirstOrDefaultAsync();
                
                if (picCrew != null)
                {
                    _logger.LogDebug("Task assigned to PIC role {Rank} ({CrewId}) via Group PIC", 
                        group.PicRole, picCrew.CrewId);
                    return picCrew.CrewId;
                }
            }

            // Tier 3: Specific crew ID override from schedule
            if (!string.IsNullOrWhiteSpace(schedule.AssignedToCrewId))
            {
                var crew = await departmentCrewQuery
                    .FirstOrDefaultAsync(c => c.CrewId == schedule.AssignedToCrewId);
                
                if (crew != null)
                {
                    _logger.LogDebug("Task assigned to crew {CrewId} via Schedule override", schedule.AssignedToCrewId);
                    return crew.CrewId;
                }
            }

            // Tier 4: Role-based assignment from schedule
            if (!string.IsNullOrWhiteSpace(schedule.AssignedToRole))
            {
                var crew = await departmentCrewQuery
                    .Where(c => c.Rank == schedule.AssignedToRole)
                    .OrderBy(c => c.CrewId) // Stable ordering
                    .FirstOrDefaultAsync();
                
                if (crew != null)
                {
                    _logger.LogDebug("Task assigned to {Rank} ({CrewId}) via Schedule role", 
                        schedule.AssignedToRole, crew.CrewId);
                    return crew.CrewId;
                }
            }

            // Tier 5: Default executor role from equipment asset
            if (!string.IsNullOrWhiteSpace(asset.DefaultExecutorRole))
            {
                var crew = await departmentCrewQuery
                    .Where(c => c.Rank == asset.DefaultExecutorRole)
                    .OrderBy(c => c.CrewId)
                    .FirstOrDefaultAsync();
                
                if (crew != null)
                {
                    _logger.LogDebug("Task assigned to {Rank} ({CrewId}) via Asset default role", 
                        asset.DefaultExecutorRole, crew.CrewId);
                    return crew.CrewId;
                }
            }

            // Tier 6: No assignment - leave for Work Planner (2/E, C/O) to assign manually
            _logger.LogDebug("Task left unassigned for manual assignment by Work Planner");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error determining task assignee, leaving unassigned");
            return null;
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Maintenance Scheduler Service is stopping");
        return base.StopAsync(cancellationToken);
    }
}
