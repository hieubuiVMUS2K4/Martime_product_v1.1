using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MaritimeEdge.Data;
using MaritimeEdge.Models;
using MaritimeEdge.DTOs;
using MaritimeEdge.Constants;
using MaritimeEdge.Services;
using MTaskStatus = MaritimeEdge.Constants.TaskStatus; // Alias to avoid ambiguity

namespace MaritimeEdge.Controllers;

[ApiController]
[Route("api/maintenance")]
public class MaintenanceController : ControllerBase
{
    private readonly EdgeDbContext _context;
    private readonly MaintenanceCompletionService _completionService;
    private readonly ILogger<MaintenanceController> _logger;

    public MaintenanceController(
        EdgeDbContext context, 
        MaintenanceCompletionService completionService,
        ILogger<MaintenanceController> logger)
    {
        _context = context;
        _completionService = completionService;
        _logger = logger;
    }

    [HttpGet("task-types")]
    public async Task<IActionResult> GetTaskTypes()
    {
        try
        {
            var taskTypes = await _context.TaskTypes
                .AsNoTracking()
                .Where(t => t.IsActive)
                .OrderBy(t => t.Category)
                .ThenBy(t => t.TypeName)
                .Select(t => new {
                    t.Id,
                    t.TypeCode,
                    t.TypeName,
                    t.Category,
                    t.DefaultPriority,
                    t.Description
                })
                .ToListAsync();

            return Ok(taskTypes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting task types");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpPost("task-types/seed")]
    public async Task<IActionResult> SeedTaskTypes()
    {
        try
        {
            // Check if already seeded
            var existingCount = await _context.TaskTypes.CountAsync();
            if (existingCount > 0)
            {
                return BadRequest(new { error = "TaskTypes already exist", count = existingCount });
            }

            var sampleTaskTypes = new List<TaskType>
            {
                // ENGINE
                new TaskType { TypeCode = "ENGINE_OIL_CHANGE", TypeName = "Engine Oil Change", Category = "ENGINE", DefaultPriority = "NORMAL", EstimatedDurationHours = 2, Description = "Change engine oil and oil filter", IsActive = true },
                new TaskType { TypeCode = "ENGINE_COOLING_CHECK", TypeName = "Cooling System Check", Category = "ENGINE", DefaultPriority = "NORMAL", EstimatedDurationHours = 1, Description = "Inspect cooling system", IsActive = true },
                new TaskType { TypeCode = "ENGINE_FUEL_FILTER", TypeName = "Fuel Filter Replacement", Category = "ENGINE", DefaultPriority = "HIGH", EstimatedDurationHours = 2, Description = "Replace fuel filters", IsActive = true },
                
                // DECK
                new TaskType { TypeCode = "DECK_WASH", TypeName = "Deck Washing", Category = "DECK", DefaultPriority = "NORMAL", EstimatedDurationHours = 2, Description = "Wash and clean deck area", IsActive = true },
                new TaskType { TypeCode = "HULL_INSPECTION", TypeName = "Hull Inspection", Category = "DECK", DefaultPriority = "HIGH", EstimatedDurationHours = 3, Description = "Inspect hull for damage", IsActive = true },
                new TaskType { TypeCode = "MOORING_CHECK", TypeName = "Mooring Equipment Check", Category = "DECK", DefaultPriority = "NORMAL", EstimatedDurationHours = 2, Description = "Inspect mooring lines", IsActive = true },
                
                // SAFETY
                new TaskType { TypeCode = "LIFEBOAT_DRILL", TypeName = "Lifeboat Drill", Category = "SAFETY", DefaultPriority = "CRITICAL", EstimatedDurationHours = 2, RequiredCertification = "Safety Officer", RequiresApproval = true, Description = "Conduct lifeboat drill", IsActive = true },
                new TaskType { TypeCode = "FIRE_EXTINGUISHER_CHECK", TypeName = "Fire Extinguisher Inspection", Category = "SAFETY", DefaultPriority = "CRITICAL", EstimatedDurationHours = 1, Description = "Inspect fire extinguishers", IsActive = true },
                new TaskType { TypeCode = "EMERGENCY_LIGHT_TEST", TypeName = "Emergency Lighting Test", Category = "SAFETY", DefaultPriority = "HIGH", EstimatedDurationHours = 1, Description = "Test emergency lights", IsActive = true },
                
                // ELECTRICAL
                new TaskType { TypeCode = "GENERATOR_MAINTENANCE", TypeName = "Generator Maintenance", Category = "ELECTRICAL", DefaultPriority = "HIGH", EstimatedDurationHours = 4, RequiredCertification = "Electrical Officer", Description = "Service generator", IsActive = true },
                new TaskType { TypeCode = "BATTERY_CHECK", TypeName = "Battery Inspection", Category = "ELECTRICAL", DefaultPriority = "NORMAL", EstimatedDurationHours = 1, Description = "Check battery condition", IsActive = true },
                new TaskType { TypeCode = "LIGHTING_INSPECTION", TypeName = "Navigation Light Inspection", Category = "ELECTRICAL", DefaultPriority = "CRITICAL", EstimatedDurationHours = 1, Description = "Test navigation lights", IsActive = true },
                
                // NAVIGATION
                new TaskType { TypeCode = "RADAR_CALIBRATION", TypeName = "Radar Calibration", Category = "NAVIGATION", DefaultPriority = "HIGH", EstimatedDurationHours = 2, RequiredCertification = "Navigation Officer", Description = "Calibrate radar", IsActive = true },
                new TaskType { TypeCode = "GPS_CHECK", TypeName = "GPS System Check", Category = "NAVIGATION", DefaultPriority = "HIGH", EstimatedDurationHours = 1, Description = "Verify GPS accuracy", IsActive = true },
                new TaskType { TypeCode = "COMPASS_ADJUSTMENT", TypeName = "Compass Adjustment", Category = "NAVIGATION", DefaultPriority = "NORMAL", EstimatedDurationHours = 2, Description = "Adjust compass", IsActive = true },
                
                // GENERAL
                new TaskType { TypeCode = "GENERAL_INSPECTION", TypeName = "General Inspection", Category = "GENERAL", DefaultPriority = "NORMAL", EstimatedDurationHours = 2, Description = "General walkthrough", IsActive = true },
                new TaskType { TypeCode = "CLEANING", TypeName = "General Cleaning", Category = "GENERAL", DefaultPriority = "NORMAL", EstimatedDurationHours = 2, Description = "Clean assigned areas", IsActive = true },
                new TaskType { TypeCode = "LUBRICATION", TypeName = "Equipment Lubrication", Category = "GENERAL", DefaultPriority = "NORMAL", EstimatedDurationHours = 1, Description = "Lubricate equipment", IsActive = true }
            };

            _context.TaskTypes.AddRange(sampleTaskTypes);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Seeded {Count} TaskTypes", sampleTaskTypes.Count);

            return Ok(new { message = "TaskTypes seeded successfully", count = sampleTaskTypes.Count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding task types");
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    /// <summary>
    /// Auto-correct task status based on due date (PMS Workflow v2.0)
    /// SCHEDULED tasks past due → DUE (if due today or in grace period) or OVERDUE (if past due)
    /// DUE tasks past grace period → OVERDUE
    /// Does not touch IN_PROGRESS, PENDING_APPROVAL, RECTIFY, or COMPLETED
    /// </summary>
    private async Task<int> AutoCorrectTaskStatuses(List<MaintenanceTask> tasks)
    {
        var now = DateTime.UtcNow;
        var today = now.Date;
        var tasksToUpdate = new List<MaintenanceTask>();

        foreach (var task in tasks)
        {
            // PMS Workflow v2.0: Handle SCHEDULED, DUE, and legacy PENDING/OVERDUE
            var statusesToProcess = new[] { "SCHEDULED", "DUE", "PENDING", "OVERDUE" };
            
            if (!statusesToProcess.Contains(task.Status))
            {
                continue; // Don't touch IN_PROGRESS, PENDING_APPROVAL, RECTIFY, COMPLETED
            }

            var dueDate = task.NextDueAt.Date;
            var isOverdue = dueDate < today;
            var isDue = dueDate <= today; // Due if today or past

            if (task.Status == "SCHEDULED")
            {
                if (isOverdue)
                {
                    task.Status = "OVERDUE";
                    tasksToUpdate.Add(task);
                }
                else if (isDue)
                {
                    task.Status = "DUE";
                    tasksToUpdate.Add(task);
                }
            }
            else if (task.Status == "DUE" && isOverdue)
            {
                task.Status = "OVERDUE";
                tasksToUpdate.Add(task);
            }
            else if (task.Status == "PENDING") // Legacy: Treat as DUE
            {
                if (isOverdue)
                {
                    task.Status = "OVERDUE";
                    tasksToUpdate.Add(task);
                }
                else
                {
                    task.Status = "DUE"; // Migrate PENDING → DUE
                    tasksToUpdate.Add(task);
                }
            }
            else if (task.Status == "OVERDUE" && !isOverdue)
            {
                // Fix incorrectly marked OVERDUE tasks
                task.Status = isDue ? "DUE" : "SCHEDULED";
                tasksToUpdate.Add(task);
            }
        }

        if (tasksToUpdate.Any())
        {
            await _context.SaveChangesAsync();
            _logger.LogInformation("Auto-corrected {Count} task statuses based on due dates (PMS Workflow v2.0)", tasksToUpdate.Count);
        }

        return tasksToUpdate.Count;
    }

    [HttpGet("tasks")]
    public async Task<IActionResult> GetAllTasks(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? status = null,
        [FromQuery] string? priority = null)
    {
        try
        {
            // Validate pagination
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 50;
            if (pageSize > 100) pageSize = 100;

            var query = _context.MaintenanceTasks.AsNoTracking().AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(t => t.Status == status);
            }

            if (!string.IsNullOrWhiteSpace(priority))
            {
                query = query.Where(t => t.Priority == priority);
            }

            // Get total count
            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            // Get paginated data with related data
            var tasks = await query
                .Include(t => t.EquipmentGroup)
                .Include(t => t.ChecklistItems)
                .OrderBy(t => t.NextDueAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                data = tasks,
                pagination = new
                {
                    currentPage = page,
                    pageSize = pageSize,
                    totalCount = totalCount,
                    totalPages = totalPages,
                    hasNextPage = page < totalPages,
                    hasPreviousPage = page > 1
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting maintenance tasks");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpGet("tasks/pending")]
    public async Task<IActionResult> GetPendingTasks()
    {
        try
        {
            var tasks = await _context.MaintenanceTasks
                .Where(t => t.Status == MTaskStatus.PENDING || t.Status == MTaskStatus.IN_PROGRESS)
                .OrderBy(t => t.NextDueAt)
                .ToListAsync();

            // Auto-correct status
            await AutoCorrectTaskStatuses(tasks);

            return Ok(tasks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending tasks");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpGet("tasks/overdue")]
    public async Task<IActionResult> GetOverdueTasks()
    {
        try
        {
            var now = DateTime.UtcNow;
            var tasks = await _context.MaintenanceTasks
                .Where(t => t.Status != MTaskStatus.COMPLETED && t.NextDueAt < now)
                .OrderBy(t => t.NextDueAt)
                .ToListAsync();

            // Auto-correct status
            await AutoCorrectTaskStatuses(tasks);

            return Ok(tasks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting overdue tasks");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Lấy danh sách task được giao cho crew member cụ thể
    /// Chỉ trả về tasks có AssignedTo chứa crew_id hoặc full_name của crew member
    /// </summary>
    [HttpGet("tasks/my-tasks")]
    public async Task<IActionResult> GetMyTasks([FromQuery] string? crewId = null, [FromQuery] string? assignedTo = null, [FromQuery] bool includeCompleted = true)
    {
        try
        {
            IQueryable<MaintenanceTask> query = _context.MaintenanceTasks;

            // IMPORTANT: Chỉ trả về tasks được assign cho crew member này
            // Nếu không có crewId và assignedTo thì trả về empty list (không trả về tất cả tasks)
            if (string.IsNullOrWhiteSpace(crewId) && string.IsNullOrWhiteSpace(assignedTo))
            {
                _logger.LogWarning("GetMyTasks called without crewId or assignedTo parameter");
                return Ok(new List<MaintenanceTask>()); // Trả về empty list thay vì tất cả tasks
            }

            // Filter by assignedTo (crew name or ID)
            if (!string.IsNullOrWhiteSpace(assignedTo))
            {
                query = query.Where(t => t.AssignedTo != null && t.AssignedTo.Contains(assignedTo));
            }
            else if (!string.IsNullOrWhiteSpace(crewId))
            {
                // If crewId is provided, try to find matching crew member
                var crew = await _context.CrewMembers
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.CrewId == crewId);
                
                if (crew != null)
                {
                    // Match by full name or crew ID (chính xác hơn)
                    // Task phải có AssignedTo chứa CrewId HOẶC FullName của crew member
                    query = query.Where(t => t.AssignedTo != null && 
                        (t.AssignedTo.Contains(crew.CrewId) || t.AssignedTo.Contains(crew.FullName)));
                    
                    _logger.LogInformation("Filtering tasks for crew: {CrewId} - {FullName}", crew.CrewId, crew.FullName);
                }
                else
                {
                    // Crew member không tồn tại, trả về empty list
                    _logger.LogWarning("Crew member not found: {CrewId}", crewId);
                    return Ok(new List<MaintenanceTask>());
                }
            }

            // Filter by status based on includeCompleted flag
            if (!includeCompleted)
            {
                // Only return pending or in-progress tasks (for TaskListScreen)
                query = query.Where(t => t.Status == MTaskStatus.PENDING || t.Status == MTaskStatus.IN_PROGRESS);
            }
            // If includeCompleted = true, return all statuses (for Dashboard)

            var tasks = await query
                .Include(t => t.ChecklistItems.OrderBy(ci => ci.SequenceOrder))
                .Include(t => t.EquipmentGroup)
                .OrderBy(t => t.NextDueAt)
                .ToListAsync();

            // Auto-correct status based on current time
            await AutoCorrectTaskStatuses(tasks);

            _logger.LogInformation("Retrieved {Count} tasks for crew: {CrewId}/{AssignedTo}, includeCompleted: {IncludeCompleted}", 
                tasks.Count, crewId, assignedTo, includeCompleted);

            return Ok(tasks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting my tasks");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpGet("tasks/{id}")]
    public async Task<IActionResult> GetTaskById(Guid id)
    {
        try
        {
            var task = await _context.MaintenanceTasks
                .Include(t => t.EquipmentGroup)
                .Include(t => t.ChecklistItems.OrderBy(ci => ci.SequenceOrder))
                .FirstOrDefaultAsync(t => t.Id == id);
            
            if (task == null)
            {
                return NotFound(new { error = "Maintenance task not found", id });
            }

            return Ok(task);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting task {Id}", id);
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    [HttpPost("tasks")]
    public async Task<IActionResult> CreateTask([FromBody] CreateMaintenanceTaskRequest request)
    {
        try
        {
            // Validate request
            if (!request.TaskTypeId.HasValue || request.TaskTypeId.Value <= 0)
            {
                return BadRequest(new { error = "Task type is required" });
            }

            // Verify TaskType exists
            var taskType = await _context.TaskTypes.FindAsync(request.TaskTypeId.Value);
            if (taskType == null)
            {
                return BadRequest(new { error = "Invalid task type" });
            }

            // Generate unique TaskId
            var taskIdPrefix = $"MT-{DateTime.UtcNow:yyyyMMdd}";
            var existingTasks = await _context.MaintenanceTasks
                .Where(t => t.TaskId.StartsWith(taskIdPrefix))
                .CountAsync();
            var taskId = $"{taskIdPrefix}-{(existingTasks + 1):D4}";

            // Create new task
            var newTask = new MaintenanceTask
            {
                TaskId = taskId,
                TaskTypeId = request.TaskTypeId,
                EquipmentId = request.EquipmentId ?? "GENERAL",
                EquipmentName = taskType.TypeName, // Use TaskType name as equipment name
                TaskType = taskType.TypeCode,
                TaskDescription = request.TaskDescription ?? taskType.Description ?? string.Empty,
                IntervalDays = request.IntervalDays,
                NextDueAt = request.NextDueAt ?? DateTime.UtcNow.AddDays(7), // Default 7 days if not specified
                Priority = request.Priority ?? taskType.DefaultPriority ?? TaskPriority.NORMAL,
                Status = MTaskStatus.PENDING, // Always start as PENDING
                AssignedTo = request.AssignedTo,
                Notes = request.Notes,
                IsSynced = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.MaintenanceTasks.Add(newTask);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created new maintenance task: {TaskId} - {TaskType} - Assigned to: {AssignedTo}", 
                newTask.TaskId, taskType.TypeName, newTask.AssignedTo ?? "Unassigned");

            return CreatedAtAction(nameof(GetTaskById), new { id = newTask.Id }, newTask);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating maintenance task");
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    [HttpPut("tasks/{id}")]
    public async Task<IActionResult> UpdateTask(Guid id, [FromBody] UpdateTaskDto dto)
    {
        try
        {
            var existing = await _context.MaintenanceTasks.FindAsync(id);
            if (existing == null)
            {
                return NotFound(new { error = "Maintenance task not found", id });
            }

            // ⚠️ IMPORTANT: Validate status transition to prevent conflicts
            if (dto.Status != existing.Status)
            {
                var validationResult = ValidateStatusTransition(existing.Status, dto.Status, existing);
                if (!validationResult.IsValid)
                {
                    return BadRequest(new { 
                        error = "Invalid status transition", 
                        message = validationResult.Message,
                        currentStatus = existing.Status,
                        attemptedStatus = dto.Status
                    });
                }
            }

            // Update properties from DTO (excludes navigation properties like ChecklistItems)
            existing.TaskId = dto.TaskId;
            existing.EquipmentId = dto.EquipmentId;
            existing.EquipmentName = dto.EquipmentName;
            existing.EquipmentGroupId = dto.EquipmentGroupId;
            existing.EquipmentGroupName = dto.EquipmentGroupName;
            existing.TaskType = dto.TaskType;
            existing.TaskDescription = dto.TaskDescription;
            existing.IntervalHours = dto.IntervalHours;
            existing.IntervalDays = dto.IntervalDays;
            existing.NextDueAt = dto.NextDueAt;
            existing.Priority = dto.Priority;
            existing.Status = dto.Status;
            existing.AssignedTo = dto.AssignedTo;
            existing.Notes = dto.Notes;
            existing.SparePartsUsed = dto.SparePartsUsed;
            existing.IsSynced = false;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated maintenance task: {Id} - {TaskId}, Status: {OldStatus} → {NewStatus}", 
                id, dto.TaskId, existing.Status, dto.Status);

            return Ok(existing);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating task {Id}", id);
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    /// <summary>
    /// PATCH /api/maintenance/tasks/{id}/status
    /// Quick status update for Kanban board drag-and-drop
    /// </summary>
    [HttpPatch("tasks/{id}/status")]
    public async Task<IActionResult> UpdateTaskStatus(Guid id, [FromBody] UpdateStatusRequest request)
    {
        try
        {
            var existing = await _context.MaintenanceTasks.FindAsync(id);
            if (existing == null)
            {
                return NotFound(new { error = "Maintenance task not found", id });
            }

            // Validate status transition
            if (request.Status != existing.Status)
            {
                var validationResult = ValidateStatusTransition(existing.Status, request.Status, existing);
                if (!validationResult.IsValid)
                {
                    return BadRequest(new { 
                        error = "Invalid status transition", 
                        message = validationResult.Message,
                        currentStatus = existing.Status,
                        attemptedStatus = request.Status
                    });
                }
            }

            // ✨ NEW: Validate equipment availability when starting maintenance
            if (request.Status == MTaskStatus.IN_PROGRESS && existing.Status != MTaskStatus.IN_PROGRESS)
            {
                var availabilityCheck = await ValidateEquipmentAvailabilityAsync(existing);
                if (!availabilityCheck.IsValid)
                {
                    return BadRequest(new { 
                        error = "Equipment not available", 
                        message = availabilityCheck.Message
                    });
                }
            }

            var oldStatus = existing.Status;
            existing.Status = request.Status;
            existing.UpdatedAt = DateTime.UtcNow;
            existing.IsSynced = false;

            // Auto-set timestamps based on status change
            if (request.Status == MTaskStatus.IN_PROGRESS && oldStatus != MTaskStatus.IN_PROGRESS)
            {
                existing.StartedAt ??= DateTime.UtcNow;
            }
            else if (request.Status == MTaskStatus.COMPLETED && oldStatus != MTaskStatus.COMPLETED)
            {
                existing.CompletedAt ??= DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            // ✨ NEW: Update equipment status after task status changes
            await UpdateEquipmentStatusForTaskAsync(existing, request.Status, oldStatus);

            _logger.LogInformation("Task {Id} status changed: {OldStatus} → {NewStatus}", 
                id, oldStatus, request.Status);

            return Ok(new { 
                id = existing.Id,
                taskId = existing.TaskId,
                status = existing.Status,
                previousStatus = oldStatus,
                message = $"Status updated from {oldStatus} to {request.Status}"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating task status {Id}", id);
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    /// <summary>
    /// Assign task to crew member (Quick Assign from Kanban board)
    /// POST /api/maintenance/tasks/{id}/assign
    /// </summary>
    [HttpPost("tasks/{id}/assign")]
    public async Task<IActionResult> AssignTask(Guid id, [FromBody] AssignTaskRequest request)
    {
        try
        {
            var task = await _context.MaintenanceTasks.FindAsync(id);
            if (task == null)
            {
                return NotFound(new { error = "Maintenance task not found", id });
            }

            // Validate crew member exists and is onboard (if crewId provided)
            if (!string.IsNullOrWhiteSpace(request.CrewId))
            {
                var crew = await _context.CrewMembers
                    .FirstOrDefaultAsync(c => c.CrewId == request.CrewId);
                
                if (crew == null)
                {
                    return BadRequest(new { error = "Crew member not found", crewId = request.CrewId });
                }

                if (!crew.IsOnboard)
                {
                    return BadRequest(new { 
                        error = "Cannot assign to crew member who is not onboard", 
                        crewId = request.CrewId,
                        crewName = crew.FullName 
                    });
                }

                task.AssignedTo = crew.CrewId;
                _logger.LogInformation("Task {TaskId} assigned to {CrewId} ({CrewName})", 
                    task.TaskId, crew.CrewId, crew.FullName);
            }
            else
            {
                // Unassign (set to null)
                task.AssignedTo = null;
                _logger.LogInformation("Task {TaskId} unassigned", task.TaskId);
            }

            task.UpdatedAt = DateTime.UtcNow;
            task.IsSynced = false;

            await _context.SaveChangesAsync();

            return Ok(new { 
                id = task.Id,
                taskId = task.TaskId,
                assignedTo = task.AssignedTo,
                message = task.AssignedTo != null 
                    ? $"Task assigned to {task.AssignedTo}" 
                    : "Task unassigned"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning task {Id}", id);
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    /// <summary>
    /// POST /api/maintenance/tasks/{id}/approve
    /// Approve or reject HIGH/CRITICAL tasks (C/E or Master only)
    /// </summary>
    [HttpPost("tasks/{id}/approve")]
    public async Task<IActionResult> ApproveTask(Guid id, [FromBody] ApproveTaskRequest request)
    {
        try
        {
            var task = await _context.MaintenanceTasks.FindAsync(id);
            if (task == null)
            {
                return NotFound(new { error = "Maintenance task not found", id });
            }

            // Validate current status is PENDING_APPROVAL
            if (task.Status != "PENDING_APPROVAL")
            {
                return BadRequest(new { 
                    error = "Task is not in PENDING_APPROVAL status", 
                    currentStatus = task.Status 
                });
            }

            // Validate approver exists and has correct rank
            var approver = await _context.CrewMembers
                .FirstOrDefaultAsync(c => c.CrewId == request.ApprovedBy);
            
            if (approver == null)
            {
                return BadRequest(new { error = "Approver not found", crewId = request.ApprovedBy });
            }

            // Get equipment group to determine department and required approver rank
            var equipmentCode = task.EquipmentId;
            var asset = await _context.EquipmentAssets
                .FirstOrDefaultAsync(a => a.AssetCode == equipmentCode);
            
            var groupMember = asset != null 
                ? await _context.EquipmentGroupMembers
                    .Include(egm => egm.Group)
                    .FirstOrDefaultAsync(egm => egm.AssetId == asset.Id)
                : null;

            var department = groupMember?.Group?.Department;
            
            // Validate approver rank based on department
            // ENGINE: C/E (Chief Engineer) or Master
            // DECK: C/O (Chief Officer) or Master
            // Others: Master only
            var validApproverRanks = new List<string>();
            
            if (department == "ENGINE")
            {
                validApproverRanks = new List<string> { "C/E", "Master" };
            }
            else if (department == "DECK")
            {
                validApproverRanks = new List<string> { "C/O", "Master" };
            }
            else
            {
                validApproverRanks = new List<string> { "Master" };
            }

            if (string.IsNullOrEmpty(approver.Rank) || !validApproverRanks.Contains(approver.Rank))
            {
                return BadRequest(new { 
                    error = $"Approver must be one of: {string.Join(", ", validApproverRanks)}", 
                    approverRank = approver.Rank ?? "N/A",
                    department = department ?? "UNKNOWN"
                });
            }

            if (request.IsApproved)
            {
                // Approve: Change status to PENDING (ready for execution)
                task.Status = "PENDING";
                task.ApprovedBy = request.ApprovedBy;
                task.ApprovedAt = DateTime.UtcNow;
                task.RejectionReason = null;
                
                _logger.LogInformation("Task {TaskId} approved by {ApprovedBy} ({Rank})", 
                    task.TaskId, request.ApprovedBy, approver.Rank);
            }
            else
            {
                // Reject: Change status to REJECTED
                if (string.IsNullOrWhiteSpace(request.RejectionReason))
                {
                    return BadRequest(new { error = "RejectionReason is required when rejecting a task" });
                }

                task.Status = "REJECTED";
                task.ApprovedBy = request.ApprovedBy;
                task.ApprovedAt = DateTime.UtcNow;
                task.RejectionReason = request.RejectionReason;
                
                _logger.LogInformation("Task {TaskId} rejected by {ApprovedBy} ({Rank}): {Reason}", 
                    task.TaskId, request.ApprovedBy, approver.Rank, request.RejectionReason);
            }

            task.UpdatedAt = DateTime.UtcNow;
            task.IsSynced = false;

            await _context.SaveChangesAsync();

            return Ok(new { 
                id = task.Id,
                taskId = task.TaskId,
                status = task.Status,
                approvedBy = task.ApprovedBy,
                approvedAt = task.ApprovedAt,
                rejectionReason = task.RejectionReason,
                message = request.IsApproved 
                    ? $"Task approved by {approver.Rank} {approver.FullName}" 
                    : $"Task rejected: {task.RejectionReason}"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving/rejecting task {Id}", id);
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    /// <summary>
    /// Validate status transition rules for Kanban board workflow
    /// Supports extended workflow: TASK → PENDING_APPROVAL → PENDING → IN_PROGRESS → COMPLETED
    /// </summary>
    private (bool IsValid, string Message) ValidateStatusTransition(string currentStatus, string newStatus, MaritimeEdge.Models.MaintenanceTask task)
    {
        // Rule 0: Same status = no transition needed
        if (currentStatus == newStatus)
        {
            return (true, "No change");
        }

        // Rule 1: COMPLETED and CANCELLED tasks cannot be moved (final states)
        if (currentStatus == MTaskStatus.COMPLETED || currentStatus == MTaskStatus.CANCELLED)
        {
            return (false, $"Cannot change status of {currentStatus.ToLower()} tasks. Create a new task if needed.");
        }

        // Define valid transitions for extended workflow
        var validTransitions = new Dictionary<string, HashSet<string>>
        {
            // TASK (new, unassigned) can go to: PENDING_APPROVAL (HIGH/CRITICAL), PENDING (LOW/NORMAL), REJECTED (direct reject)
            [MTaskStatus.TASK] = new HashSet<string> { 
                MTaskStatus.PENDING_APPROVAL, 
                MTaskStatus.PENDING, 
                MTaskStatus.REJECTED,
                MTaskStatus.CANCELLED 
            },
            
            // PENDING_APPROVAL can go to: PENDING (approved), REJECTED (rejected by C/E)
            [MTaskStatus.PENDING_APPROVAL] = new HashSet<string> { 
                MTaskStatus.PENDING, 
                MTaskStatus.REJECTED,
                MTaskStatus.TASK // Return for revision
            },
            
            // REJECTED can go to: TASK (revise and resubmit)
            [MTaskStatus.REJECTED] = new HashSet<string> { 
                MTaskStatus.TASK,
                MTaskStatus.CANCELLED
            },
            
            // PENDING can go to: IN_PROGRESS (start), OVERDUE (auto), COMPLETED (direct), TASK (unassign)
            [MTaskStatus.PENDING] = new HashSet<string> { 
                MTaskStatus.IN_PROGRESS, 
                MTaskStatus.OVERDUE, 
                MTaskStatus.COMPLETED,
                MTaskStatus.TASK,
                MTaskStatus.CANCELLED
            },
            
            // OVERDUE can go to: IN_PROGRESS (start late), PENDING (reschedule), COMPLETED (direct)
            [MTaskStatus.OVERDUE] = new HashSet<string> { 
                MTaskStatus.IN_PROGRESS, 
                MTaskStatus.PENDING, 
                MTaskStatus.COMPLETED,
                MTaskStatus.CANCELLED
            },
            
            // IN_PROGRESS can go to: PENDING (unassign), COMPLETED (finish)
            [MTaskStatus.IN_PROGRESS] = new HashSet<string> { 
                MTaskStatus.PENDING, 
                MTaskStatus.COMPLETED 
            }
        };

        // Check if transition is valid
        if (validTransitions.TryGetValue(currentStatus, out var allowedStatuses))
        {
            if (allowedStatuses.Contains(newStatus))
            {
                return (true, "Valid transition");
            }
            return (false, $"Cannot transition from {currentStatus} to {newStatus}. Allowed: {string.Join(", ", allowedStatuses)}");
        }

        // Unknown current status - allow transition (backward compatibility)
        return (true, "Valid transition (unknown source status)");
    }

    [HttpDelete("tasks/{id}")]
    public async Task<IActionResult> DeleteTask(Guid id)
    {
        try
        {
            var task = await _context.MaintenanceTasks.FindAsync(id);
            if (task == null)
            {
                return NotFound(new { error = "Maintenance task not found", id });
            }

            _context.MaintenanceTasks.Remove(task);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted maintenance task: {Id} - {TaskId} - {EquipmentName}", id, task.TaskId, task.EquipmentName);

            return Ok(new { 
                message = "Maintenance task deleted successfully", 
                id,
                taskId = task.TaskId,
                equipmentName = task.EquipmentName
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting task {Id}", id);
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    [HttpPost("tasks/{id}/start")]
    public async Task<IActionResult> StartTask(Guid id)
    {
        try
        {
            var task = await _context.MaintenanceTasks.FindAsync(id);
            if (task == null)
            {
                return NotFound(new { error = "Task not found", id });
            }

            // Check if task is already in progress or completed
            if (task.Status == MTaskStatus.IN_PROGRESS)
            {
                return BadRequest(new { error = "Task is already in progress" });
            }

            if (task.Status == MTaskStatus.COMPLETED)
            {
                return BadRequest(new { error = "Task is already completed" });
            }

            // Update task status to IN_PROGRESS
            task.Status = MTaskStatus.IN_PROGRESS;
            task.StartedAt = DateTime.UtcNow;
            task.IsSynced = false;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Started task: {Id} - {TaskId} - {EquipmentName}", id, task.TaskId, task.EquipmentName);

            return Ok(task);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting task {Id}", id);
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    [HttpPost("tasks/{id}/complete")]
    public async Task<IActionResult> CompleteTask(Guid id, [FromBody] CompleteTaskRequest request)
    {
        try
        {
            // Use MaintenanceCompletionService for automatic spare parts deduction
            var sparePartsUsed = request.SparePartsUsed != null
                ? System.Text.Json.JsonSerializer.Deserialize<List<SparePartUsage>>(request.SparePartsUsed) ?? new List<SparePartUsage>()
                : new List<SparePartUsage>();

            var result = await _completionService.CompleteTaskAsync(
                id,
                request.CompletedBy,
                sparePartsUsed,
                request.Notes,
                request.ConditionAfter
            );

            if (!result.IsSuccess)
            {
                return BadRequest(new { error = result.ErrorMessage });
            }

            var response = new
            {
                message = "Task completed successfully",
                deductedSpareParts = result.DeductedItems,
                warnings = result.Warnings
            };

            if (result.Warnings != null && result.Warnings.Any())
            {
                _logger.LogWarning("Task {TaskId} completed with warnings: {Warnings}", 
                    id, string.Join(", ", result.Warnings));
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing task");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Lấy danh sách task details (checklist) của một maintenance task
    /// GET /api/maintenance/tasks/{taskId}/checklist-legacy
    /// NOTE: Use /api/maintenance/tasks/{taskId}/checklist endpoint from TaskChecklistItemsController instead
    /// </summary>
    [HttpGet("tasks/{taskId}/checklist-legacy")]
    public async Task<IActionResult> GetTaskChecklistLegacy(Guid taskId)
    {
        try
        {
            var task = await _context.MaintenanceTasks.FindAsync(taskId);
            if (task == null)
            {
                return NotFound(new { error = "Task not found" });
            }

            // Nếu task không có TaskTypeId, trả về empty list (backward compatibility)
            if (!task.TaskTypeId.HasValue)
            {
                return Ok(new List<object>());
            }

            // Lấy task details từ TaskType
            // FIXME: TaskDetail.TaskTypeId has been removed - need to redesign relationship
            var taskDetails = new List<TaskDetail>(); // await _context.TaskDetails
                // .AsNoTracking()
                // .Where(td => td.TaskTypeId == task.TaskTypeId && td.IsActive)
                // .OrderBy(td => td.OrderIndex)
                // .ToListAsync();

            // Lấy execution status (nếu có)
            var executionDetails = await _context.MaintenanceTaskDetails
                .AsNoTracking()
                .Where(mtd => mtd.MaintenanceTaskId == taskId)
                .ToListAsync();

            // Combine data - Match Flutter's expected nested format
            var checklist = taskDetails.Select(td =>
            {
                var execution = executionDetails.FirstOrDefault(ed => ed.TaskDetailId == td.Id);
                return new
                {
                    taskDetail = new
                    {
                        id = td.Id,
                        // taskTypeId = td.TaskTypeId, // FIXME: TaskDetail.TaskTypeId removed
                        detailName = td.DetailName,
                        description = td.Description,
                        orderIndex = td.OrderIndex,
                        detailType = td.DetailType,
                        isMandatory = td.IsMandatory,
                        unit = td.Unit,
                        minValue = td.MinValue,
                        maxValue = td.MaxValue,
                        requiresPhoto = td.RequiresPhoto,
                        createdAt = td.CreatedAt
                    },
                    executionDetail = execution != null ? new
                    {
                        id = execution.Id,
                        maintenanceTaskId = execution.MaintenanceTaskId,
                        taskDetailId = execution.TaskDetailId,
                        status = execution.Status,
                        isCompleted = execution.IsCompleted,
                        measuredValue = execution.MeasuredValue,
                        checkResult = execution.CheckResult,
                        inspectionNotes = execution.Notes,
                        photoUrl = execution.PhotoUrl,
                        signatureUrl = execution.SignatureUrl,
                        completedBy = execution.CompletedBy,
                        completedAt = execution.CompletedAt
                    } : null
                };
            }).ToList();

            return Ok(checklist);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting task checklist for task {TaskId}", taskId);
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    /// <summary>
    /// Complete một task detail item trong checklist
    /// POST /api/maintenance/tasks/{taskId}/details/{detailId}/complete
    /// NOTE: Use /api/maintenance/tasks/{taskId}/checklist/{itemId}/complete from TaskChecklistItemsController instead
    /// </summary>
    [HttpPost("tasks/{taskId}/details/{detailId}/complete")]
    public async Task<IActionResult> CompleteChecklistItemLegacy(Guid taskId, long detailId, [FromBody] CompleteChecklistItemRequest request)
    {
        try
        {
            var task = await _context.MaintenanceTasks.FindAsync(taskId);
            if (task == null)
            {
                return NotFound(new { error = "Task not found" });
            }

            var taskDetail = await _context.TaskDetails.FindAsync(detailId);
            if (taskDetail == null)
            {
                return NotFound(new { error = "Task detail not found" });
            }

            // Check if execution record already exists
            var execution = await _context.MaintenanceTaskDetails
                .FirstOrDefaultAsync(mtd => mtd.MaintenanceTaskId == taskId && mtd.TaskDetailId == detailId);

            if (execution == null)
            {
                // Create new execution record
                execution = new MaritimeEdge.Models.MaintenanceTaskDetail
                {
                    MaintenanceTaskId = taskId,
                    TaskDetailId = detailId,
                    Status = MTaskStatus.COMPLETED,
                    IsCompleted = true,
                    MeasuredValue = request.MeasuredValue,
                    CheckResult = request.CheckResult,
                    Notes = request.Notes,
                    PhotoUrl = request.PhotoUrl,
                    SignatureUrl = request.SignatureUrl,
                    CompletedBy = request.CompletedBy,
                    CompletedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };
                _context.MaintenanceTaskDetails.Add(execution);
            }
            else
            {
                // Update existing execution record
                execution.Status = MTaskStatus.COMPLETED;
                execution.IsCompleted = true;
                execution.MeasuredValue = request.MeasuredValue;
                execution.CheckResult = request.CheckResult;
                execution.Notes = request.Notes;
                execution.PhotoUrl = request.PhotoUrl;
                execution.SignatureUrl = request.SignatureUrl;
                execution.CompletedBy = request.CompletedBy;
                execution.CompletedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Completed checklist item: Task {TaskId}, Detail {DetailId}", taskId, detailId);

            return Ok(execution);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing checklist item");
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    /// <summary>
    /// Lấy progress của task checklist
    /// GET /api/maintenance/tasks/{taskId}/progress
    /// </summary>
    [HttpGet("tasks/{taskId}/progress")]
    public async Task<IActionResult> GetTaskProgress(Guid taskId)
    {
        try
        {
            var task = await _context.MaintenanceTasks.FindAsync(taskId);
            if (task == null)
            {
                return NotFound(new { error = "Task not found" });
            }

            if (!task.TaskTypeId.HasValue)
            {
                return Ok(new { total = 0, completed = 0, percentage = 100 });
            }

            // Count total mandatory task details
            // FIXME: TaskDetail.TaskTypeId has been removed
            var totalMandatory = 0; // await _context.TaskDetails
                // .AsNoTracking()
                // .Where(td => td.TaskTypeId == task.TaskTypeId && td.IsActive && td.IsMandatory)
                // .CountAsync();

            // Count completed mandatory items
            var completedMandatory = await _context.MaintenanceTaskDetails
                .AsNoTracking()
                .Where(mtd => mtd.MaintenanceTaskId == taskId && mtd.IsCompleted)
                .Join(_context.TaskDetails,
                    mtd => mtd.TaskDetailId,
                    td => td.Id,
                    (mtd, td) => new { mtd, td })
                .Where(x => x.td.IsMandatory && x.td.IsActive)
                .CountAsync();

            var percentage = totalMandatory > 0 ? (int)((double)completedMandatory / totalMandatory * 100) : 0;

            return Ok(new
            {
                total = totalMandatory,
                completed = completedMandatory,
                percentage,
                canComplete = completedMandatory >= totalMandatory
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting task progress");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    public class CompleteTaskRequest
    {
        public string CompletedBy { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public string? SparePartsUsed { get; set; }
        public string? ConditionAfter { get; set; }
    }

    public class CreateMaintenanceTaskRequest
    {
        public int? TaskTypeId { get; set; } // Required - ID from TaskTypes table
        public string? EquipmentId { get; set; } // Optional - can be general
        public string? TaskDescription { get; set; }
        public int? IntervalDays { get; set; }
        public DateTime? NextDueAt { get; set; }
        public string? Priority { get; set; } // CRITICAL, HIGH, NORMAL, LOW (optional, will use TaskType default)
        public string? AssignedTo { get; set; }
        public string? Notes { get; set; }
    }

    public class CompleteChecklistItemRequest
    {
        public double? MeasuredValue { get; set; }
        public bool? CheckResult { get; set; }
        public string? Notes { get; set; }
        public string? PhotoUrl { get; set; }
        public string? SignatureUrl { get; set; }
        public string CompletedBy { get; set; } = string.Empty;
    }

    public class AssignTaskRequest
    {
        public string? CrewId { get; set; }
    }

    public class ApproveTaskRequest
    {
        public bool IsApproved { get; set; } // true = approve, false = reject
        public string? RejectionReason { get; set; } // Required if IsApproved = false
        public string ApprovedBy { get; set; } = string.Empty; // Crew ID of approver
    }

    // ==================== EQUIPMENT STATUS MANAGEMENT ====================
    
    /// <summary>
    /// Update equipment status when task status changes
    /// - IN_PROGRESS: Equipment → UNDER_MAINTENANCE
    /// - COMPLETED/CANCELLED: Equipment → ACTIVE (if no other maintenance)
    /// </summary>
    private async Task UpdateEquipmentStatusForTaskAsync(MaintenanceTask task, string newStatus, string oldStatus)
    {
        try
        {
            // Only update when transitioning to/from IN_PROGRESS or COMPLETED
            if (newStatus == oldStatus)
                return;

            var shouldSetMaintenance = newStatus == MTaskStatus.IN_PROGRESS && oldStatus != MTaskStatus.IN_PROGRESS;
            var shouldRestoreActive = (newStatus == MTaskStatus.COMPLETED || newStatus == MTaskStatus.CANCELLED) &&
                                      (oldStatus == MTaskStatus.IN_PROGRESS);

            if (!shouldSetMaintenance && !shouldRestoreActive)
                return;

            // Get equipment list (either from EquipmentGroupId or legacy EquipmentId)
            List<EquipmentAsset> equipmentList = new List<EquipmentAsset>();

            if (task.EquipmentGroupId.HasValue)
            {
                // NEW: Get all equipment in the group
                var members = await _context.EquipmentGroupMembers
                    .Where(m => m.GroupId == task.EquipmentGroupId.Value)
                    .ToListAsync();

                var assetIds = members.Select(m => m.AssetId).ToList();
                equipmentList = await _context.EquipmentAssets
                    .Where(a => assetIds.Contains(a.Id) && a.IsActive)
                    .ToListAsync();
            }
            else if (!string.IsNullOrEmpty(task.EquipmentId))
            {
                // LEGACY: Try to find equipment by AssetCode
                var equipment = await _context.EquipmentAssets
                    .FirstOrDefaultAsync(a => a.AssetCode == task.EquipmentId && a.IsActive);
                
                if (equipment != null)
                    equipmentList.Add(equipment);
            }

            if (!equipmentList.Any())
            {
                _logger.LogWarning("No equipment found for task {TaskId}", task.TaskId);
                return;
            }

            foreach (var equipment in equipmentList)
            {
                if (shouldSetMaintenance)
                {
                    // Set to UNDER_MAINTENANCE
                    _logger.LogInformation("Setting equipment {AssetCode} to UNDER_MAINTENANCE for task {TaskId}",
                        equipment.AssetCode, task.TaskId);
                    
                    equipment.Status = "UNDER_MAINTENANCE";
                    equipment.UpdatedAt = DateTime.UtcNow;
                    equipment.IsSynced = false;
                }
                else if (shouldRestoreActive)
                {
                    // Check if there are other IN_PROGRESS tasks for this equipment
                    bool hasOtherActiveMaintenance = false;

                    if (task.EquipmentGroupId.HasValue)
                    {
                        // Check if equipment group has other active tasks
                        hasOtherActiveMaintenance = await _context.MaintenanceTasks
                            .AnyAsync(t => t.EquipmentGroupId == task.EquipmentGroupId.Value &&
                                          t.Id != task.Id &&
                                          t.Status == MTaskStatus.IN_PROGRESS);
                    }
                    else
                    {
                        // Check if this specific equipment has other active tasks
                        var groupMembership = await _context.EquipmentGroupMembers
                            .Where(m => m.AssetId == equipment.Id)
                            .Select(m => m.GroupId)
                            .ToListAsync();

                        hasOtherActiveMaintenance = await _context.MaintenanceTasks
                            .AnyAsync(t => (t.EquipmentId == equipment.AssetCode ||
                                           (t.EquipmentGroupId.HasValue && groupMembership.Contains(t.EquipmentGroupId.Value))) &&
                                          t.Id != task.Id &&
                                          t.Status == MTaskStatus.IN_PROGRESS);
                    }

                    if (!hasOtherActiveMaintenance)
                    {
                        // Safe to restore to ACTIVE
                        _logger.LogInformation("Restoring equipment {AssetCode} to ACTIVE after task {TaskId} completion",
                            equipment.AssetCode, task.TaskId);
                        
                        equipment.Status = "ACTIVE";
                        equipment.UpdatedAt = DateTime.UtcNow;
                        equipment.IsSynced = false;
                    }
                    else
                    {
                        _logger.LogInformation("Equipment {AssetCode} remains UNDER_MAINTENANCE - other active tasks exist",
                            equipment.AssetCode);
                    }
                }
            }

            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating equipment status for task {TaskId}", task.TaskId);
            // Don't throw - this is a secondary operation, shouldn't block task status update
        }
    }

    /// <summary>
    /// Validate if equipment can start maintenance (not already under maintenance by critical task)
    /// </summary>
    private async Task<(bool IsValid, string? Message)> ValidateEquipmentAvailabilityAsync(MaintenanceTask task)
    {
        try
        {
            // Get equipment list
            List<EquipmentAsset> equipmentList = new List<EquipmentAsset>();

            if (task.EquipmentGroupId.HasValue)
            {
                var members = await _context.EquipmentGroupMembers
                    .Where(m => m.GroupId == task.EquipmentGroupId.Value)
                    .ToListAsync();

                var assetIds = members.Select(m => m.AssetId).ToList();
                equipmentList = await _context.EquipmentAssets
                    .Where(a => assetIds.Contains(a.Id) && a.IsActive)
                    .ToListAsync();
            }
            else if (!string.IsNullOrEmpty(task.EquipmentId))
            {
                var equipment = await _context.EquipmentAssets
                    .FirstOrDefaultAsync(a => a.AssetCode == task.EquipmentId && a.IsActive);
                
                if (equipment != null)
                    equipmentList.Add(equipment);
            }

            // Check if any equipment is under critical maintenance
            foreach (var equipment in equipmentList)
            {
                if (equipment.Status == "UNDER_MAINTENANCE")
                {
                    // Find the blocking task
                    MaintenanceTask? blockingTask = null;

                    if (task.EquipmentGroupId.HasValue)
                    {
                        blockingTask = await _context.MaintenanceTasks
                            .Where(t => t.EquipmentGroupId == task.EquipmentGroupId.Value &&
                                       t.Id != task.Id &&
                                       t.Status == MTaskStatus.IN_PROGRESS &&
                                       t.Priority == "CRITICAL")
                            .FirstOrDefaultAsync();
                    }
                    else
                    {
                        var groupIds = await _context.EquipmentGroupMembers
                            .Where(m => m.AssetId == equipment.Id)
                            .Select(m => m.GroupId)
                            .ToListAsync();

                        blockingTask = await _context.MaintenanceTasks
                            .Where(t => (t.EquipmentId == equipment.AssetCode ||
                                        (t.EquipmentGroupId.HasValue && groupIds.Contains(t.EquipmentGroupId.Value))) &&
                                       t.Id != task.Id &&
                                       t.Status == MTaskStatus.IN_PROGRESS &&
                                       t.Priority == "CRITICAL")
                            .FirstOrDefaultAsync();
                    }

                    if (blockingTask != null)
                    {
                        return (false, $"Equipment {equipment.AssetCode} is under CRITICAL maintenance (Task: {blockingTask.TaskId}). Complete critical task first.");
                    }
                }
            }

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating equipment availability for task {TaskId}", task.TaskId);
            return (true, null); // Allow on error to not block workflow
        }
    }
}
