using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MaritimeEdge.Data;
using MaritimeEdge.Models;

namespace MaritimeEdge.Controllers.Maintenance;

[ApiController]
[Route("api/maintenance/tasks/{taskId}/checklist")]
public class TaskChecklistItemsController : ControllerBase
{
    private readonly EdgeDbContext _context;
    private readonly ILogger<TaskChecklistItemsController> _logger;

    public TaskChecklistItemsController(
        EdgeDbContext context,
        ILogger<TaskChecklistItemsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all checklist items for a task
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetChecklistItems(string taskId)
    {
        try
        {
            var items = await _context.TaskChecklistItems
                .AsNoTracking()
                .Where(i => i.TaskId == taskId)
                .OrderBy(i => i.SequenceOrder)
                .ToListAsync();

            return Ok(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting checklist items for task {TaskId}", taskId);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Get single checklist item by ID
    /// </summary>
    [HttpGet("{itemId}")]
    public async Task<IActionResult> GetChecklistItem(string taskId, Guid itemId)
    {
        try
        {
            var item = await _context.TaskChecklistItems
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.Id == itemId && i.TaskId == taskId);

            if (item == null)
            {
                return NotFound(new { error = "Checklist item not found" });
            }

            return Ok(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting checklist item {ItemId}", itemId);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Update checklist item (partial update)
    /// </summary>
    [HttpPut("{itemId}")]
    public async Task<IActionResult> UpdateChecklistItem(
        string taskId,
        Guid itemId,
        [FromBody] UpdateChecklistItemRequest request)
    {
        try
        {
            var item = await _context.TaskChecklistItems
                .FirstOrDefaultAsync(i => i.Id == itemId && i.TaskId == taskId);

            if (item == null)
            {
                return NotFound(new { error = "Checklist item not found" });
            }

            // Update fields if provided
            if (request.IsCompleted.HasValue)
            {
                item.IsCompleted = request.IsCompleted.Value;
                if (request.IsCompleted.Value && !item.CompletedAt.HasValue)
                {
                    item.CompletedAt = DateTime.UtcNow;
                }
                else if (!request.IsCompleted.Value)
                {
                    item.CompletedAt = null;
                    item.CompletedBy = null;
                }
            }

            if (request.ReadingValue.HasValue)
            {
                item.ReadingValue = request.ReadingValue.Value;
            }

            if (request.Remarks != null)
            {
                item.Remarks = request.Remarks;
            }

            if (request.IsAbnormal.HasValue)
            {
                item.IsAbnormal = request.IsAbnormal.Value;
            }

            if (!string.IsNullOrWhiteSpace(request.CompletedBy))
            {
                item.CompletedBy = request.CompletedBy;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Updated checklist item {ItemId} for task {TaskId}",
                itemId, taskId);

            return Ok(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating checklist item {ItemId}", itemId);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Complete checklist item with data (convenience endpoint)
    /// </summary>
    [HttpPost("{itemId}/complete")]
    public async Task<IActionResult> CompleteChecklistItem(
        string taskId,
        Guid itemId,
        [FromBody] CompleteChecklistItemRequest request)
    {
        try
        {
            var item = await _context.TaskChecklistItems
                .FirstOrDefaultAsync(i => i.Id == itemId && i.TaskId == taskId);

            if (item == null)
            {
                return NotFound(new { error = "Checklist item not found" });
            }

            // Mark as completed
            item.IsCompleted = true;
            item.CompletedAt = DateTime.UtcNow;
            item.CompletedBy = request.CompletedBy;

            // Update data
            if (request.ReadingValue.HasValue)
            {
                item.ReadingValue = request.ReadingValue.Value;
            }

            if (!string.IsNullOrWhiteSpace(request.Remarks))
            {
                item.Remarks = request.Remarks;
            }

            item.IsAbnormal = request.IsAbnormal;

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Completed checklist item {ItemId} for task {TaskId} by {CompletedBy}",
                itemId, taskId, request.CompletedBy);

            return Ok(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing checklist item {ItemId}", itemId);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Get checklist completion summary
    /// </summary>
    [HttpGet("summary")]
    public async Task<IActionResult> GetChecklistSummary(string taskId)
    {
        try
        {
            var items = await _context.TaskChecklistItems
                .AsNoTracking()
                .Where(i => i.TaskId == taskId)
                .ToListAsync();

            var summary = new
            {
                totalCount = items.Count,
                completedCount = items.Count(i => i.IsCompleted),
                abnormalCount = items.Count(i => i.IsAbnormal && !i.IsCompleted),
                completionPercentage = items.Count > 0
                    ? Math.Round((items.Count(i => i.IsCompleted) / (double)items.Count) * 100, 2)
                    : 0
            };

            return Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting checklist summary for task {TaskId}", taskId);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }
}

// Request DTOs
public class UpdateChecklistItemRequest
{
    public bool? IsCompleted { get; set; }
    public double? ReadingValue { get; set; }
    public string? Remarks { get; set; }
    public bool? IsAbnormal { get; set; }
    public string? CompletedBy { get; set; }
}

public class CompleteChecklistItemRequest
{
    public required string CompletedBy { get; set; }
    public double? ReadingValue { get; set; }
    public string? Remarks { get; set; }
    public bool IsAbnormal { get; set; } = false;
}
