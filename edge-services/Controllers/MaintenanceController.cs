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

    [HttpGet("tasks")]
    public async Task<IActionResult> GetAllTasks()
    {
        try
        {
            var tasks = await _context.MaintenanceTasks
                .OrderBy(t => t.NextDueAt)
                .ToListAsync();

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

            return Ok(tasks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting overdue tasks");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpGet("tasks/my-tasks")]
    public async Task<IActionResult> GetMyTasks([FromQuery] string? crewId = null, [FromQuery] string? assignedTo = null, [FromQuery] bool includeCompleted = true)
    {
        try
        {
            IQueryable<MaintenanceTask> query = _context.MaintenanceTasks;

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
                    // Match by full name or crew ID
                    query = query.Where(t => t.AssignedTo != null && 
                        (t.AssignedTo.Contains(crew.FullName) || t.AssignedTo.Contains(crew.CrewId)));
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

            _logger.LogInformation("Updated maintenance task: {Id} - {TaskId}", id, task.TaskId);

            return Ok(existing);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating task {Id}", id);
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
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

    public class CompleteTaskRequest
    {
        public string CompletedBy { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public string? SparePartsUsed { get; set; }
    }
}
