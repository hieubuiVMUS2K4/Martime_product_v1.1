using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MaritimeEdge.Data;
using MaritimeEdge.Models;
using System.Text.Json;

namespace MaritimeEdge.Controllers.Maintenance;

[ApiController]
[Route("api/maintenance/tasks/{taskId}")]
public class TaskRealTimeUpdatesController : ControllerBase
{
    private readonly EdgeDbContext _context;
    private readonly ILogger<TaskRealTimeUpdatesController> _logger;

    public TaskRealTimeUpdatesController(
        EdgeDbContext context,
        ILogger<TaskRealTimeUpdatesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Update spare parts used during task execution (for real-time visibility)
    /// This allows edge frontend to see spare parts being used while task is in progress
    /// </summary>
    [HttpPut("spare-parts")]
    public async Task<IActionResult> UpdateSparePartsUsed(
        string taskId,
        [FromBody] UpdateSparePartsRequest request)
    {
        try
        {
            var task = await _context.MaintenanceTasks
                .FirstOrDefaultAsync(t => t.TaskId == taskId);

            if (task == null)
            {
                return NotFound(new { error = "Task not found" });
            }

            // Update spare parts field for real-time visibility
            task.SparePartsUsed = request.SparePartsUsed;
            task.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Updated spare parts for task {TaskId} - Parts: {PartsCount}",
                taskId,
                request.SparePartsUsed?.Length ?? 0);

            return Ok(new { 
                message = "Spare parts updated successfully",
                taskId = taskId,
                updatedAt = task.UpdatedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating spare parts for task {TaskId}", taskId);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Update task progress indicators for real-time kanban updates
    /// </summary>
    [HttpPost("progress-update")]
    public async Task<IActionResult> UpdateTaskProgress(
        string taskId,
        [FromBody] TaskProgressUpdateRequest request)
    {
        try
        {
            var task = await _context.MaintenanceTasks
                .FirstOrDefaultAsync(t => t.TaskId == taskId);

            if (task == null)
            {
                return NotFound(new { error = "Task not found" });
            }

            // Update progress fields if provided
            if (request.ProgressNotes != null)
            {
                task.Notes = request.ProgressNotes;
            }

            if (request.CurrentRunningHours.HasValue)
            {
                // Store current running hours as a note or additional field
                var progressInfo = new
                {
                    currentRunningHours = request.CurrentRunningHours.Value,
                    updatedAt = DateTime.UtcNow
                };
                
                // You could store this in a separate progress tracking table
                // or append to notes field
                var existingNotes = task.Notes ?? "";
                task.Notes = existingNotes + $"\n[Progress Update: {JsonSerializer.Serialize(progressInfo)}]";
            }

            task.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Updated progress for task {TaskId}",
                taskId);

            return Ok(new { 
                message = "Task progress updated successfully",
                taskId = taskId,
                updatedAt = task.UpdatedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating progress for task {TaskId}", taskId);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }
}

public class UpdateSparePartsRequest
{
    public string? SparePartsUsed { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class TaskProgressUpdateRequest
{
    public string? ProgressNotes { get; set; }
    public double? CurrentRunningHours { get; set; }
    public DateTime? UpdatedAt { get; set; }
}