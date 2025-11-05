using Microsoft.AspNetCore.Mvc;
using MaritimeEdge.Services;
using MaritimeEdge.DTOs;

namespace MaritimeEdge.Controllers;

/// <summary>
/// Task Management Controller
/// Quản lý Task Types và Task Details
/// </summary>
[ApiController]
[Route("api/task-management")]
public class TaskManagementController : ControllerBase
{
    private readonly TaskManagementService _taskService;
    private readonly ILogger<TaskManagementController> _logger;

    public TaskManagementController(TaskManagementService taskService, ILogger<TaskManagementController> logger)
    {
        _taskService = taskService;
        _logger = logger;
    }

    // ============================================================
    // TASK TYPES ENDPOINTS
    // ============================================================

    /// <summary>
    /// Lấy tất cả Task Types
    /// GET /api/task-management/task-types
    /// </summary>
    [HttpGet("task-types")]
    public async Task<ActionResult<List<TaskTypeDto>>> GetAllTaskTypes([FromQuery] bool activeOnly = true)
    {
        try
        {
            var taskTypes = await _taskService.GetAllTaskTypesAsync(activeOnly);
            return Ok(new { success = true, data = taskTypes, count = taskTypes.Count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting task types");
            return StatusCode(500, new { success = false, message = "Lỗi server: " + ex.Message });
        }
    }

    /// <summary>
    /// Lấy Task Type theo ID
    /// GET /api/task-management/task-types/{id}
    /// </summary>
    [HttpGet("task-types/{id}")]
    public async Task<ActionResult<TaskTypeDto>> GetTaskTypeById(int id)
    {
        try
        {
            var taskType = await _taskService.GetTaskTypeByIdAsync(id);
            if (taskType == null)
            {
                return NotFound(new { success = false, message = "Không tìm thấy Task Type" });
            }

            return Ok(new { success = true, data = taskType });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting task type {Id}", id);
            return StatusCode(500, new { success = false, message = "Lỗi server: " + ex.Message });
        }
    }

    /// <summary>
    /// Lấy Task Types theo category
    /// GET /api/task-management/task-types/category/{category}
    /// </summary>
    [HttpGet("task-types/category/{category}")]
    public async Task<ActionResult<List<TaskTypeDto>>> GetTaskTypesByCategory(string category)
    {
        try
        {
            var taskTypes = await _taskService.GetTaskTypesByCategoryAsync(category);
            return Ok(new { success = true, data = taskTypes, count = taskTypes.Count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting task types by category {Category}", category);
            return StatusCode(500, new { success = false, message = "Lỗi server: " + ex.Message });
        }
    }

    /// <summary>
    /// Lấy Task Type với tất cả Details
    /// GET /api/task-management/task-types/{id}/with-details
    /// </summary>
    [HttpGet("task-types/{id}/with-details")]
    public async Task<ActionResult<TaskTypeWithDetailsDto>> GetTaskTypeWithDetails(int id)
    {
        try
        {
            var result = await _taskService.GetTaskTypeWithDetailsAsync(id);
            if (result == null)
            {
                return NotFound(new { success = false, message = "Không tìm thấy Task Type" });
            }

            return Ok(new { success = true, data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting task type with details {Id}", id);
            return StatusCode(500, new { success = false, message = "Lỗi server: " + ex.Message });
        }
    }

    /// <summary>
    /// Tạo Task Type mới
    /// POST /api/task-management/task-types
    /// </summary>
    [HttpPost("task-types")]
    public async Task<ActionResult<TaskTypeDto>> CreateTaskType([FromBody] CreateTaskTypeDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ", errors = ModelState });
            }

            var (success, message, taskType) = await _taskService.CreateTaskTypeAsync(dto);

            if (!success)
            {
                return BadRequest(new { success = false, message });
            }

            return CreatedAtAction(
                nameof(GetTaskTypeById),
                new { id = taskType!.Id },
                new { success = true, message, data = taskType }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating task type");
            return StatusCode(500, new { success = false, message = "Lỗi server: " + ex.Message });
        }
    }

    /// <summary>
    /// Cập nhật Task Type
    /// PUT /api/task-management/task-types/{id}
    /// </summary>
    [HttpPut("task-types/{id}")]
    public async Task<ActionResult> UpdateTaskType(int id, [FromBody] UpdateTaskTypeDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ", errors = ModelState });
            }

            var (success, message) = await _taskService.UpdateTaskTypeAsync(id, dto);

            if (!success)
            {
                return BadRequest(new { success = false, message });
            }

            return Ok(new { success = true, message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating task type {Id}", id);
            return StatusCode(500, new { success = false, message = "Lỗi server: " + ex.Message });
        }
    }

    /// <summary>
    /// Xóa Task Type (soft delete)
    /// DELETE /api/task-management/task-types/{id}
    /// </summary>
    [HttpDelete("task-types/{id}")]
    public async Task<ActionResult> DeleteTaskType(int id)
    {
        try
        {
            var (success, message) = await _taskService.DeleteTaskTypeAsync(id);

            if (!success)
            {
                return BadRequest(new { success = false, message });
            }

            return Ok(new { success = true, message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting task type {Id}", id);
            return StatusCode(500, new { success = false, message = "Lỗi server: " + ex.Message });
        }
    }

    /// <summary>
    /// Thống kê Task Types theo category
    /// GET /api/task-management/task-types/stats/by-category
    /// </summary>
    [HttpGet("task-types/stats/by-category")]
    public async Task<ActionResult> GetTaskTypeStatsByCategory()
    {
        try
        {
            var stats = await _taskService.GetTaskTypeStatsByCategoryAsync();
            return Ok(new { success = true, data = stats });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting task type stats");
            return StatusCode(500, new { success = false, message = "Lỗi server: " + ex.Message });
        }
    }

    // ============================================================
    // TASK DETAILS ENDPOINTS
    // ============================================================

    /// <summary>
    /// Lấy TẤT CẢ Task Details (không phân biệt Task Type)
    /// GET /api/task-management/task-details/all
    /// </summary>
    [HttpGet("task-details/all")]
    public async Task<ActionResult<List<TaskDetailDto>>> GetAllTaskDetails([FromQuery] bool activeOnly = true)
    {
        try
        {
            var details = await _taskService.GetAllTaskDetailsAsync(activeOnly);
            return Ok(new { success = true, data = details, count = details.Count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all task details");
            return StatusCode(500, new { success = false, message = "Lỗi server: " + ex.Message });
        }
    }

    /// <summary>
    /// Lấy tất cả Task Details của một Task Type
    /// GET /api/task-management/task-types/{taskTypeId}/details
    /// </summary>
    [HttpGet("task-types/{taskTypeId}/details")]
    public async Task<ActionResult<List<TaskDetailDto>>> GetTaskDetailsByType(int taskTypeId, [FromQuery] bool activeOnly = true)
    {
        try
        {
            var details = await _taskService.GetTaskDetailsByTypeIdAsync(taskTypeId, activeOnly);
            return Ok(new { success = true, data = details, count = details.Count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting task details for type {TaskTypeId}", taskTypeId);
            return StatusCode(500, new { success = false, message = "Lỗi server: " + ex.Message });
        }
    }

    /// <summary>
    /// Lấy Task Detail theo ID
    /// GET /api/task-management/task-details/{id}
    /// </summary>
    [HttpGet("task-details/{id}")]
    public async Task<ActionResult<TaskDetailDto>> GetTaskDetailById(long id)
    {
        try
        {
            var detail = await _taskService.GetTaskDetailByIdAsync(id);
            if (detail == null)
            {
                return NotFound(new { success = false, message = "Không tìm thấy Task Detail" });
            }

            return Ok(new { success = true, data = detail });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting task detail {Id}", id);
            return StatusCode(500, new { success = false, message = "Lỗi server: " + ex.Message });
        }
    }

    /// <summary>
    /// Tạo Task Detail mới
    /// POST /api/task-management/task-details
    /// </summary>
    [HttpPost("task-details")]
    public async Task<ActionResult<TaskDetailDto>> CreateTaskDetail([FromBody] CreateTaskDetailDto dto)
    {
        try
        {
            _logger.LogInformation("Creating TaskDetail - Payload: {@Dto}", dto);
            
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState Invalid: {@Errors}", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ", errors = ModelState });
            }

            var (success, message, detail) = await _taskService.CreateTaskDetailAsync(dto);

            if (!success)
            {
                return BadRequest(new { success = false, message });
            }

            return CreatedAtAction(
                nameof(GetTaskDetailById),
                new { id = detail!.Id },
                new { success = true, message, data = detail }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating task detail");
            return StatusCode(500, new { success = false, message = "Lỗi server: " + ex.Message });
        }
    }

    /// <summary>
    /// Cập nhật Task Detail
    /// PUT /api/task-management/task-details/{id}
    /// </summary>
    [HttpPut("task-details/{id}")]
    public async Task<ActionResult> UpdateTaskDetail(long id, [FromBody] UpdateTaskDetailDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ", errors = ModelState });
            }

            var (success, message) = await _taskService.UpdateTaskDetailAsync(id, dto);

            if (!success)
            {
                return BadRequest(new { success = false, message });
            }

            return Ok(new { success = true, message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating task detail {Id}", id);
            return StatusCode(500, new { success = false, message = "Lỗi server: " + ex.Message });
        }
    }

    /// <summary>
    /// Xóa Task Detail
    /// DELETE /api/task-management/task-details/{id}
    /// </summary>
    [HttpDelete("task-details/{id}")]
    public async Task<ActionResult> DeleteTaskDetail(long id)
    {
        try
        {
            var (success, message) = await _taskService.DeleteTaskDetailAsync(id);

            if (!success)
            {
                return BadRequest(new { success = false, message });
            }

            return Ok(new { success = true, message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting task detail {Id}", id);
            return StatusCode(500, new { success = false, message = "Lỗi server: " + ex.Message });
        }
    }

    /// <summary>
    /// Sắp xếp lại thứ tự Task Details
    /// PUT /api/task-management/task-types/{taskTypeId}/details/reorder
    /// </summary>
    [HttpPut("task-types/{taskTypeId}/details/reorder")]
    public async Task<ActionResult> ReorderTaskDetails(int taskTypeId, [FromBody] ReorderTaskDetailsDto dto)
    {
        try
        {
            var (success, message) = await _taskService.ReorderTaskDetailsAsync(taskTypeId, dto);

            if (!success)
            {
                return BadRequest(new { success = false, message });
            }

            return Ok(new { success = true, message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reordering task details for type {TaskTypeId}", taskTypeId);
            return StatusCode(500, new { success = false, message = "Lỗi server: " + ex.Message });
        }
    }

    // ============================================================
    // UTILITY ENDPOINTS
    // ============================================================

    /// <summary>
    /// Lấy danh sách categories
    /// GET /api/task-management/categories
    /// </summary>
    [HttpGet("categories")]
    public ActionResult GetCategories()
    {
        var categories = new[]
        {
            new { code = "ENGINE", name = "Động cơ" },
            new { code = "SAFETY", name = "An toàn" },
            new { code = "DECK", name = "Boong" },
            new { code = "ELECTRICAL", name = "Điện" },
            new { code = "NAVIGATION", name = "Hàng hải" },
            new { code = "CARGO", name = "Hàng hóa" },
            new { code = "GENERAL", name = "Chung" }
        };

        return Ok(new { success = true, data = categories });
    }

    /// <summary>
    /// Lấy danh sách priorities
    /// GET /api/task-management/priorities
    /// </summary>
    [HttpGet("priorities")]
    public ActionResult GetPriorities()
    {
        var priorities = new[]
        {
            new { code = "CRITICAL", name = "Khẩn cấp", level = 1 },
            new { code = "HIGH", name = "Cao", level = 2 },
            new { code = "NORMAL", name = "Trung bình", level = 3 },
            new { code = "LOW", name = "Thấp", level = 4 }
        };

        return Ok(new { success = true, data = priorities });
    }

    /// <summary>
    /// Lấy danh sách detail types
    /// GET /api/task-management/detail-types
    /// </summary>
    [HttpGet("detail-types")]
    public ActionResult GetDetailTypes()
    {
        var detailTypes = new[]
        {
            new { code = "CHECKLIST", name = "Checklist (Check/Uncheck)", description = "Công việc cần check (true/false)" },
            new { code = "MEASUREMENT", name = "Đo đạc (Measurement)", description = "Đo đạc với giá trị số (nhiệt độ, áp suất, etc.)" },
            new { code = "INSPECTION", name = "Kiểm tra (Inspection)", description = "Kiểm tra bằng mắt, ghi nhận tình trạng" }
        };

        return Ok(new { success = true, data = detailTypes });
    }

    /// <summary>
    /// Health check
    /// GET /api/task-management/health
    /// </summary>
    [HttpGet("health")]
    public async Task<ActionResult> Health()
    {
        try
        {
            var taskTypes = await _taskService.GetAllTaskTypesAsync(true);
            var stats = await _taskService.GetTaskTypeStatsByCategoryAsync();

            return Ok(new
            {
                success = true,
                message = "Task Management System is healthy",
                stats = new
                {
                    totalTaskTypes = taskTypes.Count,
                    categories = stats
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking task management health");
            return StatusCode(500, new { success = false, message = "Lỗi server: " + ex.Message });
        }
    }
}
