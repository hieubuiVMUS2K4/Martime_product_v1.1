using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MaritimeEdge.Data;
using MaritimeEdge.Models;
using MaritimeEdge.DTOs;
using System.Text.Json;
using MTaskStatus = MaritimeEdge.Constants.TaskStatus;

namespace MaritimeEdge.Controllers;

[ApiController]
[Route("api/deferral-requests")]
public class DeferralRequestController : ControllerBase
{
    private readonly EdgeDbContext _context;
    private readonly ILogger<DeferralRequestController> _logger;

    public DeferralRequestController(EdgeDbContext context, ILogger<DeferralRequestController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Create a new deferral request
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateDeferralRequest([FromBody] CreateDeferralRequestDto dto)
    {
        try
        {
            _logger.LogInformation("=== CreateDeferralRequest START ===");
            _logger.LogInformation("TaskId: {TaskId}", dto.TaskId);
            _logger.LogInformation("Reason length: {Length}", dto.Reason?.Length ?? 0);
            _logger.LogInformation("ProposedDueDate: {Date}", dto.ProposedDueDate);
            _logger.LogInformation("Attachments count: {Count}", dto.Attachments?.Count ?? 0);
            
            // Get user ID from header (set by mobile app or frontend)
            var userId = Request.Headers["X-User-Id"].FirstOrDefault() ?? "SYSTEM";
            var deviceType = Request.Headers["X-Device-Type"].FirstOrDefault() ?? "WEB";

            // Validate task exists and is in valid status
            var task = await _context.MaintenanceTasks
                .AsNoTracking() // Prevent circular reference in sync queue
                .FirstOrDefaultAsync(t => t.Id == dto.TaskId && !t.IsDeleted);

            if (task == null)
            {
                _logger.LogWarning("Task not found: {TaskId}", dto.TaskId);
                return NotFound(new { error = "Task not found" });
            }

            _logger.LogInformation("Found task: {TaskId}, Status: {Status}, NextDueAt: {NextDueAt}", 
                task.TaskId, task.Status, task.NextDueAt);
            
            // No need to re-attach - we'll update directly without loading nav properties
            // Just update the specific fields we need

            // Can only defer tasks in SCHEDULED, DUE, OVERDUE, or MISSING_* statuses
            // MISSING_* statuses are warnings, not workflow states - still eligible for deferral
            var allowedStatuses = new[] { "SCHEDULED", "DUE", "OVERDUE", "MISSING_CHECKLIST", "MISSING_PIC", "MISSING_BOTH" };
            if (!allowedStatuses.Contains(task.Status))
            {
                return BadRequest(new { 
                    error = "Cannot defer task in current status",
                    currentStatus = task.Status,
                    allowedStatuses = allowedStatuses,
                    hint = "Only tasks that are scheduled, due, overdue, or missing setup can be deferred"
                });
            }

            // OVERDUE or overdue MISSING_* tasks require stricter validation
            var isOverdue = task.Status == "OVERDUE" || 
                            (task.Status.StartsWith("MISSING_") && task.NextDueAt < DateTime.UtcNow);
            var isOverdueDeferral = isOverdue;
            if (isOverdueDeferral)
            {
                // Require longer, more detailed reason
                if (dto.Reason.Length < 50)
                {
                    return BadRequest(new {
                        error = "OVERDUE tasks require detailed explanation (minimum 50 characters)",
                        provided = dto.Reason.Length,
                        required = 50
                    });
                }

                // Require attachments (proof of issue)
                if (dto.Attachments == null || dto.Attachments.Count == 0)
                {
                    return BadRequest(new {
                        error = "OVERDUE task deferrals require photo/document attachments as proof",
                        hint = "Please provide evidence of the issue (e.g., spare parts order, weather report, Class email)"
                    });
                }

                // Require root cause and preventive measures in reason
                var reasonLower = dto.Reason.ToLower();
                if (string.IsNullOrWhiteSpace(dto.RootCause) || dto.RootCause.Length < 20)
                {
                    return BadRequest(new {
                        error = "OVERDUE deferrals require root cause analysis (minimum 20 characters)",
                        hint = "Explain why the task became overdue"
                    });
                }

                if (string.IsNullOrWhiteSpace(dto.PreventiveMeasures) || dto.PreventiveMeasures.Length < 20)
                {
                    return BadRequest(new {
                        error = "OVERDUE deferrals require preventive measures (minimum 20 characters)",
                        hint = "Explain how you will prevent this from happening again"
                    });
                }
            }

            // Check for existing pending deferral
            if (task.HasPendingDeferral)
            {
                var existingRequest = await _context.TaskDeferralRequests
                    .Where(d => d.TaskId == dto.TaskId && d.Status == "PENDING")
                    .FirstOrDefaultAsync();

                if (existingRequest != null)
                {
                    return BadRequest(new { 
                        error = "Task already has a pending deferral request",
                        existingRequestId = existingRequest.Id
                    });
                }
            }

            // Validate proposed date

            if (dto.ProposedDueDate <= task.NextDueAt)
            {
                _logger.LogWarning("Invalid proposed date: {Proposed} <= {Current}", dto.ProposedDueDate, task.NextDueAt);
                return BadRequest(new { 
                    error = "Proposed due date must be after current due date",
                    currentDueDate = task.NextDueAt,
                    proposedDueDate = dto.ProposedDueDate
                });
            }

            var deferralDays = (int)(dto.ProposedDueDate - task.NextDueAt).TotalDays;
            _logger.LogInformation("Deferral days calculated: {Days}", deferralDays);

            // CMS validation: require Class Permission Letter if deferral > 90 days
            if (task.IsCms && deferralDays > 90 && string.IsNullOrEmpty(dto.ClassPermissionLetter))
            {
                return BadRequest(new { 
                    error = "CMS items require Class Permission Letter for deferrals over 90 days",
                    deferralDays = deferralDays,
                    maxWithoutPermission = 90
                });
            }

            // Create deferral request
            _logger.LogInformation("Creating deferral request object...");
            
            // Serialize attachments with size check
            string? attachmentsJson = null;
            if (dto.Attachments != null && dto.Attachments.Count > 0)
            {
                try
                {
                    _logger.LogInformation("Serializing {Count} attachments...", dto.Attachments.Count);
                    var totalSize = dto.Attachments.Sum(a => a.Length);
                    _logger.LogInformation("Total attachments size: {Size} bytes ({MB} MB)", totalSize, totalSize / 1024.0 / 1024.0);
                    
                    if (totalSize > 10 * 1024 * 1024) // 10MB limit
                    {
                        _logger.LogWarning("Attachments too large: {Size} MB", totalSize / 1024.0 / 1024.0);
                        return BadRequest(new {
                            error = "Attachments too large - maximum 10MB total",
                            totalSize = $"{totalSize / 1024.0 / 1024.0:F2} MB"
                        });
                    }
                    
                    attachmentsJson = JsonSerializer.Serialize(dto.Attachments);
                    _logger.LogInformation("✅ Attachments serialized successfully");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to serialize attachments");
                    return StatusCode(500, new {
                        error = "Failed to process attachments",
                        message = ex.Message
                    });
                }
            }
            
            var deferralRequest = new TaskDeferralRequest
            {
                Id = Guid.NewGuid(),
                TaskId = dto.TaskId,
                RequestedBy = userId,
                RequestedAt = DateTime.UtcNow,
                Reason = dto.Reason,
                CurrentDueDate = task.NextDueAt,
                ProposedDueDate = dto.ProposedDueDate,
                DeferralDays = deferralDays,
                Status = "PENDING",
                Priority = isOverdueDeferral ? "HIGH" : dto.Priority,
                IsCmsItem = task.IsCms,
                ClassPermissionLetter = dto.ClassPermissionLetter,
                Attachments = attachmentsJson,
                IsOverdueDeferral = isOverdueDeferral,
                RootCause = dto.RootCause,
                PreventiveMeasures = dto.PreventiveMeasures,
                TaskStatusAtRequest = task.Status,
                OriginNode = task.OriginNode,
                IsSynced = true // Skip sync queue to avoid circular reference
            };

            // Update task directly via ExecuteSqlRaw to avoid loading nav properties
            _logger.LogInformation("Updating task {TaskId} via SQL - setting HasPendingDeferral = true", task.TaskId);
            await _context.Database.ExecuteSqlRawAsync(
                "UPDATE maintenance_tasks SET has_pending_deferral = true, updated_at = {0} WHERE id = {1}",
                DateTime.UtcNow, dto.TaskId
            );

            // Add status history entry
            var statusHistory = new TaskStatusHistory
            {
                Id = Guid.NewGuid(),
                TaskId = dto.TaskId,
                FromStatus = null,
                ToStatus = "DEFERRAL_REQUESTED",
                ChangedBy = userId,
                ChangedAt = DateTime.UtcNow,
                Reason = $"Deferral requested: {dto.Reason.Substring(0, Math.Min(100, dto.Reason.Length))}...",
                DeviceType = deviceType
            };

            _context.TaskDeferralRequests.Add(deferralRequest);
            _context.TaskStatusHistories.Add(statusHistory);
            
            _logger.LogInformation("Saving to database...");
            await _context.SaveChangesAsync();
            _logger.LogInformation("✅ Deferral request saved successfully");

            _logger.LogInformation("Deferral request created for task {TaskId} by {UserId}", task.TaskId, userId);

            return Ok(new { 
                message = "Deferral request created successfully",
                requestId = deferralRequest.Id,
                taskId = task.TaskId,
                deferralDays = deferralDays,
                status = "PENDING"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error creating deferral request - Exception: {Message}", ex.Message);
            _logger.LogError("Stack trace: {StackTrace}", ex.StackTrace);
            if (ex.InnerException != null)
            {
                _logger.LogError("Inner exception: {InnerMessage}", ex.InnerException.Message);
            }
            return StatusCode(500, new { 
                error = "Internal server error",
                message = ex.Message,
                type = ex.GetType().Name
            });
        }
    }

    /// <summary>
    /// Get list of deferral requests with filtering
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetDeferralRequests([FromQuery] DeferralRequestQueryDto query)
    {
        try
        {
            var queryable = _context.TaskDeferralRequests
                .AsNoTracking()
                .Include(d => d.Task)
                .AsQueryable();

            // Filters
            if (!string.IsNullOrEmpty(query.Status))
            {
                queryable = queryable.Where(d => d.Status == query.Status);
            }

            if (!string.IsNullOrEmpty(query.RequestedBy))
            {
                queryable = queryable.Where(d => d.RequestedBy == query.RequestedBy);
            }

            if (query.TaskId.HasValue)
            {
                queryable = queryable.Where(d => d.TaskId == query.TaskId.Value);
            }

            if (query.IsCmsOnly == true)
            {
                queryable = queryable.Where(d => d.IsCmsItem);
            }

            // Count total
            var totalCount = await queryable.CountAsync();

            // Sort
            queryable = query.SortBy?.ToLower() switch
            {
                "priority" => query.SortOrder == "asc" 
                    ? queryable.OrderBy(d => d.Priority) 
                    : queryable.OrderByDescending(d => d.Priority),
                "proposedduedate" => query.SortOrder == "asc" 
                    ? queryable.OrderBy(d => d.ProposedDueDate) 
                    : queryable.OrderByDescending(d => d.ProposedDueDate),
                "status" => query.SortOrder == "asc" 
                    ? queryable.OrderBy(d => d.Status) 
                    : queryable.OrderByDescending(d => d.Status),
                _ => query.SortOrder == "asc" 
                    ? queryable.OrderBy(d => d.RequestedAt) 
                    : queryable.OrderByDescending(d => d.RequestedAt)
            };

            // Pagination
            var skip = (query.Page - 1) * query.PageSize;
            var items = await queryable
                .Skip(skip)
                .Take(query.PageSize)
                .ToListAsync();

            // Get crew names for display
            var crewIds = items
                .SelectMany(d => new[] { d.RequestedBy, d.ReviewedBy })
                .Where(id => !string.IsNullOrEmpty(id))
                .Distinct()
                .ToList();

            var crewNames = await _context.CrewMembers
                .Where(c => crewIds.Contains(c.CrewId))
                .ToDictionaryAsync(c => c.CrewId, c => c.FullName);

            var dtos = items.Select(d => new DeferralRequestDto
            {
                Id = d.Id,
                TaskId = d.TaskId,
                TaskCode = d.Task.TaskId,
                TaskDescription = d.Task.TaskDescription,
                EquipmentName = d.Task.EquipmentName ?? d.Task.EquipmentGroupName,
                RequestedBy = d.RequestedBy,
                RequestedByName = crewNames.GetValueOrDefault(d.RequestedBy),
                RequestedAt = d.RequestedAt,
                Reason = d.Reason,
                CurrentDueDate = d.CurrentDueDate,
                ProposedDueDate = d.ProposedDueDate,
                DeferralDays = d.DeferralDays,
                Status = d.Status,
                Priority = d.Priority,
                IsCmsItem = d.IsCmsItem,
                ClassPermissionLetter = d.ClassPermissionLetter,
                ReviewedBy = d.ReviewedBy,
                ReviewedByName = d.ReviewedBy != null ? crewNames.GetValueOrDefault(d.ReviewedBy) : null,
                ReviewedAt = d.ReviewedAt,
                ReviewNotes = d.ReviewNotes,
                Attachments = !string.IsNullOrEmpty(d.Attachments) 
                    ? JsonSerializer.Deserialize<List<string>>(d.Attachments) 
                    : null,
                CreatedAt = d.CreatedAt
            }).ToList();

            return Ok(new
            {
                items = dtos,
                totalCount,
                page = query.Page,
                pageSize = query.PageSize,
                totalPages = (int)Math.Ceiling((double)totalCount / query.PageSize)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting deferral requests");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Get single deferral request details
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetDeferralRequest(Guid id)
    {
        try
        {
            var deferral = await _context.TaskDeferralRequests
                .AsNoTracking()
                .Include(d => d.Task)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (deferral == null)
            {
                return NotFound(new { error = "Deferral request not found" });
            }

            // Get crew names
            var crewIds = new List<string?> { deferral.RequestedBy, deferral.ReviewedBy }
                .Where(id => !string.IsNullOrEmpty(id))
                .ToList();

            var crewNames = await _context.CrewMembers
                .Where(c => crewIds.Contains(c.CrewId))
                .ToDictionaryAsync(c => c.CrewId, c => c.FullName);

            var dto = new DeferralRequestDto
            {
                Id = deferral.Id,
                TaskId = deferral.TaskId,
                TaskCode = deferral.Task.TaskId,
                TaskDescription = deferral.Task.TaskDescription,
                EquipmentName = deferral.Task.EquipmentName ?? deferral.Task.EquipmentGroupName,
                RequestedBy = deferral.RequestedBy,
                RequestedByName = crewNames.GetValueOrDefault(deferral.RequestedBy),
                RequestedAt = deferral.RequestedAt,
                Reason = deferral.Reason,
                CurrentDueDate = deferral.CurrentDueDate,
                ProposedDueDate = deferral.ProposedDueDate,
                DeferralDays = deferral.DeferralDays,
                Status = deferral.Status,
                Priority = deferral.Priority,
                IsCmsItem = deferral.IsCmsItem,
                ClassPermissionLetter = deferral.ClassPermissionLetter,
                ReviewedBy = deferral.ReviewedBy,
                ReviewedByName = deferral.ReviewedBy != null ? crewNames.GetValueOrDefault(deferral.ReviewedBy) : null,
                ReviewedAt = deferral.ReviewedAt,
                ReviewNotes = deferral.ReviewNotes,
                Attachments = !string.IsNullOrEmpty(deferral.Attachments) 
                    ? JsonSerializer.Deserialize<List<string>>(deferral.Attachments) 
                    : null,
                CreatedAt = deferral.CreatedAt
            };

            return Ok(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting deferral request {Id}", id);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Approve or Reject a deferral request (C/E or Master only)
    /// </summary>
    [HttpPut("{id:guid}/review")]
    public async Task<IActionResult> ReviewDeferralRequest(Guid id, [FromBody] ReviewDeferralRequestDto dto)
    {
        try
        {
            var userId = Request.Headers["X-User-Id"].FirstOrDefault() ?? "SYSTEM";
            var deviceType = Request.Headers["X-Device-Type"].FirstOrDefault() ?? "WEB";

            var deferral = await _context.TaskDeferralRequests
                .Include(d => d.Task)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (deferral == null)
            {
                return NotFound(new { error = "Deferral request not found" });
            }

            if (deferral.Status != "PENDING")
            {
                return BadRequest(new { 
                    error = "Deferral request is not pending",
                    currentStatus = deferral.Status
                });
            }

            var action = dto.Action.ToUpper();
            if (action != "APPROVE" && action != "REJECT")
            {
                return BadRequest(new { error = "Action must be APPROVE or REJECT" });
            }

            var task = deferral.Task;

            if (action == "APPROVE")
            {
                // Apply deferral
                var newDueDate = dto.AdjustedDueDate ?? deferral.ProposedDueDate;
                var wasOverdue = task.Status == "OVERDUE";
                
                task.NextDueAt = newDueDate;
                task.HasPendingDeferral = false;
                task.DeferralCount += 1;
                task.LastDeferredAt = DateTime.UtcNow;
                task.LastDeferredBy = userId;
                
                // Reset status if OVERDUE (giving fresh start)
                if (wasOverdue)
                {
                    task.Status = "DUE";
                }
                
                task.UpdatedAt = DateTime.UtcNow;

                deferral.Status = "APPROVED";
                deferral.ReviewedBy = userId;
                deferral.ReviewedAt = DateTime.UtcNow;
                deferral.ReviewNotes = dto.Notes;
                deferral.UpdatedAt = DateTime.UtcNow;

                // Add status history
                var statusHistory = new TaskStatusHistory
                {
                    Id = Guid.NewGuid(),
                    TaskId = task.Id,
                    FromStatus = wasOverdue ? "OVERDUE" : task.Status,
                    ToStatus = wasOverdue ? "DUE" : task.Status,
                    ChangedBy = userId,
                    ChangedAt = DateTime.UtcNow,
                    Reason = wasOverdue 
                        ? $"OVERDUE deferral approved. Status reset to DUE. New due date: {newDueDate:yyyy-MM-dd}" 
                        : $"Deferral approved. New due date: {newDueDate:yyyy-MM-dd}",
                    Notes = dto.Notes,
                    DeviceType = deviceType
                };
                _context.TaskStatusHistories.Add(statusHistory);

                _logger.LogInformation(
                    "Deferral request {Id} approved for task {TaskId} by {UserId}. {StatusChange}", 
                    id, task.TaskId, userId, 
                    wasOverdue ? "Status reset from OVERDUE to DUE" : "Status unchanged"
                );
            }
            else // REJECT
            {
                task.HasPendingDeferral = false;
                task.UpdatedAt = DateTime.UtcNow;

                deferral.Status = "REJECTED";
                deferral.ReviewedBy = userId;
                deferral.ReviewedAt = DateTime.UtcNow;
                deferral.ReviewNotes = dto.Notes;
                deferral.UpdatedAt = DateTime.UtcNow;

                // Add status history
                var statusHistory = new TaskStatusHistory
                {
                    Id = Guid.NewGuid(),
                    TaskId = task.Id,
                    FromStatus = null,
                    ToStatus = "DEFERRAL_REJECTED",
                    ChangedBy = userId,
                    ChangedAt = DateTime.UtcNow,
                    Reason = $"Deferral request rejected: {dto.Notes}",
                    DeviceType = deviceType
                };
                _context.TaskStatusHistories.Add(statusHistory);

                _logger.LogInformation("Deferral request {Id} rejected for task {TaskId} by {UserId}", 
                    id, task.TaskId, userId);
            }

            await _context.SaveChangesAsync();

            return Ok(new { 
                message = $"Deferral request {action.ToLower()}d",
                requestId = id,
                taskId = task.TaskId,
                newStatus = deferral.Status,
                newDueDate = action == "APPROVE" ? task.NextDueAt : (DateTime?)null
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reviewing deferral request {Id}", id);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Cancel a pending deferral request (by requester only)
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> CancelDeferralRequest(Guid id)
    {
        try
        {
            var userId = Request.Headers["X-User-Id"].FirstOrDefault() ?? "SYSTEM";

            var deferral = await _context.TaskDeferralRequests
                .Include(d => d.Task)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (deferral == null)
            {
                return NotFound(new { error = "Deferral request not found" });
            }

            if (deferral.Status != "PENDING")
            {
                return BadRequest(new { 
                    error = "Can only cancel pending requests",
                    currentStatus = deferral.Status
                });
            }

            // Update task
            deferral.Task.HasPendingDeferral = false;
            deferral.Task.UpdatedAt = DateTime.UtcNow;

            // Remove deferral request
            _context.TaskDeferralRequests.Remove(deferral);

            // Add status history
            var statusHistory = new TaskStatusHistory
            {
                Id = Guid.NewGuid(),
                TaskId = deferral.TaskId,
                FromStatus = null,
                ToStatus = "DEFERRAL_CANCELLED",
                ChangedBy = userId,
                ChangedAt = DateTime.UtcNow,
                Reason = "Deferral request cancelled by requester"
            };
            _context.TaskStatusHistories.Add(statusHistory);

            await _context.SaveChangesAsync();

            _logger.LogInformation("Deferral request {Id} cancelled by {UserId}", id, userId);

            return Ok(new { message = "Deferral request cancelled" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling deferral request {Id}", id);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Get pending deferral count for dashboard
    /// </summary>
    [HttpGet("pending/count")]
    public async Task<IActionResult> GetPendingCount()
    {
        try
        {
            var count = await _context.TaskDeferralRequests
                .CountAsync(d => d.Status == "PENDING");

            var cmsCount = await _context.TaskDeferralRequests
                .CountAsync(d => d.Status == "PENDING" && d.IsCmsItem);

            return Ok(new { 
                pendingCount = count,
                cmsCount = cmsCount
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending deferral count");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }
}
