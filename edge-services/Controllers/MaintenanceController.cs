using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MaritimeEdge.Data;
using MaritimeEdge.Models;

namespace MaritimeEdge.Controllers;

[ApiController]
[Route("api/maintenance")]
public class MaintenanceController : ControllerBase
{
    private readonly EdgeDbContext _context;
    private readonly ILogger<MaintenanceController> _logger;

    public MaintenanceController(EdgeDbContext context, ILogger<MaintenanceController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet("task-types")]
    public async Task<IActionResult> GetTaskTypes()
    {
        try
        {
            var taskTypes = await _context.TaskTypes
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
    /// Auto-correct task status based on due date
    /// PENDING tasks past due → OVERDUE
    /// OVERDUE tasks not yet due → PENDING
    /// Does not touch IN_PROGRESS or COMPLETED
    /// </summary>
    private async Task<int> AutoCorrectTaskStatuses(List<MaintenanceTask> tasks)
    {
        var now = DateTime.UtcNow;
        var tasksToUpdate = new List<MaintenanceTask>();

        foreach (var task in tasks)
        {
            // Only update PENDING and OVERDUE statuses (don't touch IN_PROGRESS or COMPLETED)
            if (task.Status == "PENDING" || task.Status == "OVERDUE")
            {
                var shouldBeOverdue = task.NextDueAt < now;
                
                if (shouldBeOverdue && task.Status != "OVERDUE")
                {
                    task.Status = "OVERDUE";
                    tasksToUpdate.Add(task);
                }
                else if (!shouldBeOverdue && task.Status == "OVERDUE")
                {
                    // Fix incorrectly marked OVERDUE tasks
                    task.Status = "PENDING";
                    tasksToUpdate.Add(task);
                }
            }
        }

        if (tasksToUpdate.Any())
        {
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Auto-corrected {tasksToUpdate.Count} task statuses based on due dates");
        }

        return tasksToUpdate.Count;
    }

    [HttpGet("tasks")]
    public async Task<IActionResult> GetAllTasks()
    {
        try
        {
            var tasks = await _context.MaintenanceTasks
                .OrderBy(t => t.NextDueAt)
                .ToListAsync();

            // Auto-correct status based on current time
            await AutoCorrectTaskStatuses(tasks);

            return Ok(tasks);
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
                .Where(t => t.Status == "PENDING" || t.Status == "IN_PROGRESS")
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
                .Where(t => t.Status != "COMPLETED" && t.NextDueAt < now)
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
                query = query.Where(t => t.Status == "PENDING" || t.Status == "IN_PROGRESS");
            }
            // If includeCompleted = true, return all statuses (for Dashboard)

            var tasks = await query
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
    public async Task<IActionResult> GetTaskById(long id)
    {
        try
        {
            var task = await _context.MaintenanceTasks.FindAsync(id);
            
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
                Priority = request.Priority ?? taskType.DefaultPriority ?? "NORMAL",
                Status = "PENDING", // Always start as PENDING
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
    public async Task<IActionResult> UpdateTask(long id, [FromBody] MaintenanceTask task)
    {
        try
        {
            var existing = await _context.MaintenanceTasks.FindAsync(id);
            if (existing == null)
            {
                return NotFound(new { error = "Maintenance task not found", id });
            }

            // ⚠️ IMPORTANT: Validate status transition to prevent conflicts
            if (task.Status != existing.Status)
            {
                var validationResult = ValidateStatusTransition(existing.Status, task.Status, existing);
                if (!validationResult.IsValid)
                {
                    return BadRequest(new { 
                        error = "Invalid status transition", 
                        message = validationResult.Message,
                        currentStatus = existing.Status,
                        attemptedStatus = task.Status
                    });
                }
            }

            // Update all properties
            existing.TaskId = task.TaskId;
            existing.EquipmentId = task.EquipmentId;
            existing.EquipmentName = task.EquipmentName;
            existing.TaskType = task.TaskType;
            existing.TaskDescription = task.TaskDescription;
            existing.IntervalHours = task.IntervalHours;
            existing.IntervalDays = task.IntervalDays;
            existing.NextDueAt = task.NextDueAt;
            existing.Priority = task.Priority;
            existing.Status = task.Status;
            existing.AssignedTo = task.AssignedTo;
            existing.Notes = task.Notes;
            existing.SparePartsUsed = task.SparePartsUsed;
            existing.IsSynced = false;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated maintenance task: {Id} - {TaskId}, Status: {OldStatus} → {NewStatus}", 
                id, task.TaskId, existing.Status, task.Status);

            return Ok(existing);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating task {Id}", id);
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    /// <summary>
    /// Validate status transition rules to prevent conflicts between mobile and web
    /// </summary>
    private (bool IsValid, string Message) ValidateStatusTransition(string currentStatus, string newStatus, MaritimeEdge.Models.MaintenanceTask task)
    {
        // Rule 1: COMPLETED tasks cannot be moved back (protect crew work)
        if (currentStatus == "COMPLETED")
        {
            return (false, "Cannot change status of completed tasks. Task was completed by crew member. Create a new task if rework is needed.");
        }

        // Rule 2: IN_PROGRESS can move to PENDING (Captain unassigns) or COMPLETED (Crew finishes)
        // Allow IN_PROGRESS → PENDING: Captain can unassign a task that crew started (StartedAt timestamp is kept)
        // Allow IN_PROGRESS → COMPLETED: Crew finishes the task via mobile
        if (currentStatus == "IN_PROGRESS" && newStatus != "PENDING" && newStatus != "COMPLETED")
        {
            return (false, $"In-progress tasks can only move to PENDING (unassign) or COMPLETED, not {newStatus}");
        }

        // Rule 3: PENDING can move to IN_PROGRESS (assign) or OVERDUE (auto by system)
        if (currentStatus == "PENDING" && newStatus != "IN_PROGRESS" && newStatus != "OVERDUE")
        {
            return (false, $"Pending tasks can only move to IN_PROGRESS or OVERDUE, not {newStatus}");
        }

        // Rule 4: OVERDUE can only go to IN_PROGRESS (crew starts late task)
        if (currentStatus == "OVERDUE" && newStatus != "IN_PROGRESS")
        {
            return (false, "Overdue tasks must be started (IN_PROGRESS) before completion");
        }

        return (true, "Valid transition");
    }

    [HttpDelete("tasks/{id}")]
    public async Task<IActionResult> DeleteTask(long id)
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
    public async Task<IActionResult> StartTask(long id)
    {
        try
        {
            var task = await _context.MaintenanceTasks.FindAsync(id);
            if (task == null)
            {
                return NotFound(new { error = "Task not found", id });
            }

            // Check if task is already in progress or completed
            if (task.Status == "IN_PROGRESS")
            {
                return BadRequest(new { error = "Task is already in progress" });
            }

            if (task.Status == "COMPLETED")
            {
                return BadRequest(new { error = "Task is already completed" });
            }

            // Update task status to IN_PROGRESS
            task.Status = "IN_PROGRESS";
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
    public async Task<IActionResult> CompleteTask(long id, [FromBody] CompleteTaskRequest request)
    {
        try
        {
            var task = await _context.MaintenanceTasks.FindAsync(id);
            if (task == null)
            {
                return NotFound(new { message = "Task not found" });
            }

            task.Status = "COMPLETED";
            task.CompletedAt = DateTime.UtcNow;
            task.CompletedBy = request.CompletedBy;
            task.Notes = request.Notes;
            task.SparePartsUsed = request.SparePartsUsed;
            task.LastDoneAt = DateTime.UtcNow;
            
            // Calculate next due date
            if (task.IntervalDays.HasValue)
            {
                task.NextDueAt = DateTime.UtcNow.AddDays(task.IntervalDays.Value);
            }

            await _context.SaveChangesAsync();

            return Ok(task);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing task");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Lấy danh sách task details (checklist) của một maintenance task
    /// GET /api/maintenance/tasks/{taskId}/checklist
    /// </summary>
    [HttpGet("tasks/{taskId}/checklist")]
    public async Task<IActionResult> GetTaskChecklist(long taskId)
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

            // Lấy task details từ TaskType using many-to-many relationship
            var taskDetails = await _context.TaskDetails
                .Where(td => td.TaskTypes.Any(tt => tt.Id == task.TaskTypeId) && td.IsActive)
                .OrderBy(td => td.OrderIndex)
                .ToListAsync();

            // Lấy execution status (nếu có)
            var executionDetails = await _context.MaintenanceTaskDetails
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
                        taskTypeId = task.TaskTypeId, // Use the task's TaskTypeId instead
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
    /// POST /api/maintenance/tasks/{taskId}/checklist/{detailId}/complete
    /// </summary>
    [HttpPost("tasks/{taskId}/checklist/{detailId}/complete")]
    public async Task<IActionResult> CompleteChecklistItem(long taskId, long detailId, [FromBody] CompleteChecklistItemRequest request)
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
                    Status = "COMPLETED",
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
                execution.Status = "COMPLETED";
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
    public async Task<IActionResult> GetTaskProgress(long taskId)
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

            // Count total mandatory task details using many-to-many relationship
            var totalMandatory = await _context.TaskDetails
                .Where(td => td.TaskTypes.Any(tt => tt.Id == task.TaskTypeId) && td.IsActive && td.IsMandatory)
                .CountAsync();

            // Count completed mandatory items
            var completedMandatory = await _context.MaintenanceTaskDetails
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
}
