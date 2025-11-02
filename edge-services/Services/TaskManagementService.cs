using Microsoft.EntityFrameworkCore;
using MaritimeEdge.Data;
using MaritimeEdge.Models;
using MaritimeEdge.DTOs;

namespace MaritimeEdge.Services;

/// <summary>
/// Service quản lý Task Types và Task Details
/// </summary>
public class TaskManagementService
{
    private readonly EdgeDbContext _context;
    private readonly ILogger<TaskManagementService> _logger;

    public TaskManagementService(EdgeDbContext context, ILogger<TaskManagementService> logger)
    {
        _context = context;
        _logger = logger;
    }

    // ============================================================
    // TASK TYPES METHODS
    // ============================================================

    /// <summary>
    /// Lấy tất cả Task Types với thống kê
    /// </summary>
    public async Task<List<TaskTypeDto>> GetAllTaskTypesAsync(bool activeOnly = true)
    {
        var query = _context.TaskTypes.AsQueryable();

        if (activeOnly)
        {
            query = query.Where(t => t.IsActive);
        }

        var taskTypes = await query
            .OrderBy(t => t.Category)
            .ThenBy(t => t.TypeName)
            .ToListAsync();

        var result = new List<TaskTypeDto>();

        foreach (var tt in taskTypes)
        {
            var detailStats = await _context.TaskDetails
                .Where(td => td.TaskTypeId == tt.Id && td.IsActive)
                .GroupBy(td => 1)
                .Select(g => new
                {
                    Total = g.Count(),
                    Mandatory = g.Count(td => td.IsMandatory),
                    Photo = g.Count(td => td.RequiresPhoto),
                    Signature = g.Count(td => td.RequiresSignature)
                })
                .FirstOrDefaultAsync();

            result.Add(new TaskTypeDto
            {
                Id = tt.Id,
                TypeCode = tt.TypeCode,
                TypeName = tt.TypeName,
                Description = tt.Description,
                Category = tt.Category,
                DefaultPriority = tt.DefaultPriority,
                EstimatedDurationHours = tt.EstimatedDurationHours,
                RequiredCertification = tt.RequiredCertification,
                RequiresApproval = tt.RequiresApproval,
                IsActive = tt.IsActive,
                CreatedAt = tt.CreatedAt,
                UpdatedAt = tt.UpdatedAt,
                TotalDetails = detailStats?.Total ?? 0,
                MandatoryDetails = detailStats?.Mandatory ?? 0,
                PhotoRequiredDetails = detailStats?.Photo ?? 0,
                SignatureRequiredDetails = detailStats?.Signature ?? 0
            });
        }

        return result;
    }

    /// <summary>
    /// Lấy Task Type theo ID
    /// </summary>
    public async Task<TaskTypeDto?> GetTaskTypeByIdAsync(int id)
    {
        var taskType = await _context.TaskTypes.FindAsync(id);
        if (taskType == null) return null;

        var detailStats = await _context.TaskDetails
            .Where(td => td.TaskTypeId == id && td.IsActive)
            .GroupBy(td => 1)
            .Select(g => new
            {
                Total = g.Count(),
                Mandatory = g.Count(td => td.IsMandatory),
                Photo = g.Count(td => td.RequiresPhoto),
                Signature = g.Count(td => td.RequiresSignature)
            })
            .FirstOrDefaultAsync();

        return new TaskTypeDto
        {
            Id = taskType.Id,
            TypeCode = taskType.TypeCode,
            TypeName = taskType.TypeName,
            Description = taskType.Description,
            Category = taskType.Category,
            DefaultPriority = taskType.DefaultPriority,
            EstimatedDurationHours = taskType.EstimatedDurationHours,
            RequiredCertification = taskType.RequiredCertification,
            RequiresApproval = taskType.RequiresApproval,
            IsActive = taskType.IsActive,
            CreatedAt = taskType.CreatedAt,
            UpdatedAt = taskType.UpdatedAt,
            TotalDetails = detailStats?.Total ?? 0,
            MandatoryDetails = detailStats?.Mandatory ?? 0,
            PhotoRequiredDetails = detailStats?.Photo ?? 0,
            SignatureRequiredDetails = detailStats?.Signature ?? 0
        };
    }

    /// <summary>
    /// Lấy Task Types theo category
    /// </summary>
    public async Task<List<TaskTypeDto>> GetTaskTypesByCategoryAsync(string category)
    {
        var taskTypes = await GetAllTaskTypesAsync(true);
        return taskTypes.Where(t => t.Category.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    /// <summary>
    /// Tạo Task Type mới
    /// </summary>
    public async Task<(bool Success, string Message, TaskTypeDto? TaskType)> CreateTaskTypeAsync(CreateTaskTypeDto dto)
    {
        try
        {
            // Kiểm tra trùng TypeCode
            var exists = await _context.TaskTypes.AnyAsync(t => t.TypeCode == dto.TaskTypeCode);
            if (exists)
            {
                return (false, $"Type code '{dto.TaskTypeCode}' đã tồn tại", null);
            }

            var taskType = new TaskType
            {
                TypeCode = dto.TaskTypeCode.ToUpper(),
                TypeName = dto.TypeName,
                Description = dto.Description,
                Category = dto.Category.ToUpper(),
                DefaultPriority = dto.Priority.ToUpper(),
                EstimatedDurationHours = dto.EstimatedDurationMinutes.HasValue 
                    ? (int)Math.Round(dto.EstimatedDurationMinutes.Value / 60.0) 
                    : null,
                RequiredCertification = dto.RequiredCertification,
                RequiresApproval = dto.RequiresApproval,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.TaskTypes.Add(taskType);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created TaskType: {TypeCode} - {TypeName}", taskType.TypeCode, taskType.TypeName);

            var result = await GetTaskTypeByIdAsync(taskType.Id);
            return (true, "Tạo Task Type thành công", result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating TaskType");
            return (false, $"Lỗi: {ex.Message}", null);
        }
    }

    /// <summary>
    /// Cập nhật Task Type
    /// </summary>
    public async Task<(bool Success, string Message)> UpdateTaskTypeAsync(int id, UpdateTaskTypeDto dto)
    {
        try
        {
            var taskType = await _context.TaskTypes.FindAsync(id);
            if (taskType == null)
            {
                return (false, "Không tìm thấy Task Type");
            }

            // Update only non-null properties
            if (dto.TypeName != null) taskType.TypeName = dto.TypeName;
            if (dto.Description != null) taskType.Description = dto.Description;
            if (dto.Category != null) taskType.Category = dto.Category.ToUpper();
            if (dto.Priority != null) taskType.DefaultPriority = dto.Priority.ToUpper();
            if (dto.EstimatedDurationMinutes.HasValue) 
            {
                taskType.EstimatedDurationHours = (int)Math.Round(dto.EstimatedDurationMinutes.Value / 60.0);
            }
            if (dto.RequiredCertification != null) taskType.RequiredCertification = dto.RequiredCertification;
            if (dto.RequiresApproval.HasValue) taskType.RequiresApproval = dto.RequiresApproval.Value;
            if (dto.IsActive.HasValue) taskType.IsActive = dto.IsActive.Value;
            taskType.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated TaskType: {Id} - {TypeName}", id, taskType.TypeName);

            return (true, "Cập nhật Task Type thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating TaskType");
            return (false, $"Lỗi: {ex.Message}");
        }
    }

    /// <summary>
    /// Xóa Task Type (soft delete)
    /// </summary>
    public async Task<(bool Success, string Message)> DeleteTaskTypeAsync(int id)
    {
        try
        {
            var taskType = await _context.TaskTypes.FindAsync(id);
            if (taskType == null)
            {
                return (false, "Không tìm thấy Task Type");
            }

            // Kiểm tra xem có MaintenanceTask nào đang sử dụng không
            var inUse = await _context.MaintenanceTasks.AnyAsync(mt => mt.TaskTypeId == id);
            if (inUse)
            {
                return (false, "Không thể xóa Task Type đang được sử dụng bởi MaintenanceTask");
            }

            taskType.IsActive = false;
            taskType.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted TaskType: {Id} - {TypeName}", id, taskType.TypeName);

            return (true, "Xóa Task Type thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting TaskType");
            return (false, $"Lỗi: {ex.Message}");
        }
    }

    // ============================================================
    // TASK DETAILS METHODS
    // ============================================================

    /// <summary>
    /// Lấy TẤT CẢ Task Details (không phân biệt TaskType)
    /// Dùng cho thư viện Details
    /// </summary>
    public async Task<List<TaskDetailDto>> GetAllTaskDetailsAsync(bool activeOnly = true)
    {
        var query = _context.TaskDetails.AsQueryable();

        if (activeOnly)
        {
            query = query.Where(td => td.IsActive);
        }

        var details = await query
            .OrderBy(td => td.CreatedAt)
            .ToListAsync();

        var taskTypeIds = details.Select(td => td.TaskTypeId).Distinct().ToList();
        var taskTypes = await _context.TaskTypes
            .Where(tt => taskTypeIds.Contains(tt.Id))
            .ToDictionaryAsync(tt => tt.Id, tt => tt.TypeName);

        return details.Select(td => new TaskDetailDto
        {
            Id = td.Id,
            TaskTypeId = td.TaskTypeId ?? 0,
            TaskTypeName = td.TaskTypeId.HasValue && taskTypes.ContainsKey(td.TaskTypeId.Value) 
                ? taskTypes[td.TaskTypeId.Value] 
                : "Thư viện",
            DetailName = td.DetailName,
            Description = td.Description,
            OrderIndex = td.OrderIndex,
            DetailType = td.DetailType,
            IsMandatory = td.IsMandatory,
            Unit = td.Unit,
            MinValue = td.MinValue,
            MaxValue = td.MaxValue,
            RequiresPhoto = td.RequiresPhoto,
            RequiresSignature = td.RequiresSignature,
            Instructions = td.Instructions,
            IsActive = td.IsActive,
            CreatedAt = td.CreatedAt
        }).ToList();
    }

    /// <summary>
    /// Lấy tất cả Task Details của một Task Type
    /// </summary>
    public async Task<List<TaskDetailDto>> GetTaskDetailsByTypeIdAsync(int taskTypeId, bool activeOnly = true)
    {
        var query = _context.TaskDetails
            .Where(td => td.TaskTypeId == taskTypeId);

        if (activeOnly)
        {
            query = query.Where(td => td.IsActive);
        }

        var details = await query
            .OrderBy(td => td.OrderIndex)
            .ToListAsync();

        var taskType = await _context.TaskTypes.FindAsync(taskTypeId);

        return details.Select(td => new TaskDetailDto
        {
            Id = td.Id,
            TaskTypeId = td.TaskTypeId ?? 0,
            TaskTypeName = taskType?.TypeName ?? "",
            DetailName = td.DetailName,
            Description = td.Description,
            OrderIndex = td.OrderIndex,
            DetailType = td.DetailType,
            IsMandatory = td.IsMandatory,
            Unit = td.Unit,
            MinValue = td.MinValue,
            MaxValue = td.MaxValue,
            RequiresPhoto = td.RequiresPhoto,
            RequiresSignature = td.RequiresSignature,
            Instructions = td.Instructions,
            IsActive = td.IsActive,
            CreatedAt = td.CreatedAt
        }).ToList();
    }

    /// <summary>
    /// Lấy Task Detail theo ID
    /// </summary>
    public async Task<TaskDetailDto?> GetTaskDetailByIdAsync(long id)
    {
        var detail = await _context.TaskDetails.FindAsync(id);
        if (detail == null) return null;

        TaskType? taskType = null;
        if (detail.TaskTypeId.HasValue)
        {
            taskType = await _context.TaskTypes.FindAsync(detail.TaskTypeId.Value);
        }

        return new TaskDetailDto
        {
            Id = detail.Id,
            TaskTypeId = detail.TaskTypeId ?? 0,
            TaskTypeName = taskType?.TypeName ?? "Thư viện",
            DetailName = detail.DetailName,
            Description = detail.Description,
            OrderIndex = detail.OrderIndex,
            DetailType = detail.DetailType,
            IsMandatory = detail.IsMandatory,
            Unit = detail.Unit,
            MinValue = detail.MinValue,
            MaxValue = detail.MaxValue,
            RequiresPhoto = detail.RequiresPhoto,
            RequiresSignature = detail.RequiresSignature,
            Instructions = detail.Instructions,
            IsActive = detail.IsActive,
            CreatedAt = detail.CreatedAt
        };
    }

    /// <summary>
    /// Tạo Task Detail mới (có thể standalone cho library hoặc gắn với TaskType)
    /// </summary>
    public async Task<(bool Success, string Message, TaskDetailDto? Detail)> CreateTaskDetailAsync(CreateTaskDetailDto dto)
    {
        try
        {
            // Convert 0 to null (frontend may send 0 instead of null)
            if (dto.TaskTypeId.HasValue && dto.TaskTypeId.Value == 0)
            {
                dto.TaskTypeId = null;
            }

            // Nếu có TaskTypeId, kiểm tra tồn tại
            if (dto.TaskTypeId.HasValue && dto.TaskTypeId.Value > 0)
            {
                var taskType = await _context.TaskTypes.FindAsync(dto.TaskTypeId.Value);
                if (taskType == null)
                {
                    return (false, "Không tìm thấy Task Type", null);
                }
            }

            var detail = new TaskDetail
            {
                TaskTypeId = dto.TaskTypeId, // null = library detail (standalone)
                DetailName = dto.DetailName,
                Description = dto.Description,
                OrderIndex = dto.OrderIndex,
                DetailType = dto.DetailType.ToUpper(),
                IsMandatory = dto.IsMandatory,
                Unit = dto.Unit,
                MinValue = dto.MinValue,
                MaxValue = dto.MaxValue,
                RequiresPhoto = dto.RequiresPhoto,
                RequiresSignature = dto.RequiresSignature,
                Instructions = dto.Instructions,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.TaskDetails.Add(detail);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created TaskDetail: {Id} - {DetailName} (TaskTypeId: {TaskTypeId})", 
                detail.Id, detail.DetailName, detail.TaskTypeId?.ToString() ?? "Library");

            var result = await GetTaskDetailByIdAsync(detail.Id);
            return (true, "Tạo Task Detail thành công", result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating TaskDetail");
            return (false, $"Lỗi: {ex.Message}", null);
        }
    }

    /// <summary>
    /// Cập nhật Task Detail
    /// </summary>
    public async Task<(bool Success, string Message)> UpdateTaskDetailAsync(long id, UpdateTaskDetailDto dto)
    {
        try
        {
            var detail = await _context.TaskDetails.FindAsync(id);
            if (detail == null)
            {
                return (false, "Không tìm thấy Task Detail");
            }

            // Update only non-null properties
            // IMPORTANT: Allow updating TaskTypeId to assign/unassign from TaskType
            if (dto.TaskTypeId.HasValue) 
            {
                detail.TaskTypeId = dto.TaskTypeId.Value;
                _logger.LogInformation("Updated TaskDetail {Id} TaskTypeId to {TaskTypeId}", id, dto.TaskTypeId.Value);
            }
            
            if (dto.DetailName != null) detail.DetailName = dto.DetailName;
            if (dto.Description != null) detail.Description = dto.Description;
            if (dto.OrderIndex.HasValue) detail.OrderIndex = dto.OrderIndex.Value;
            if (dto.DetailType != null) detail.DetailType = dto.DetailType.ToUpper();
            if (dto.IsMandatory.HasValue) detail.IsMandatory = dto.IsMandatory.Value;
            if (dto.Unit != null) detail.Unit = dto.Unit;
            if (dto.MinValue.HasValue) detail.MinValue = dto.MinValue;
            if (dto.MaxValue.HasValue) detail.MaxValue = dto.MaxValue;
            if (dto.RequiresPhoto.HasValue) detail.RequiresPhoto = dto.RequiresPhoto.Value;
            if (dto.RequiresSignature.HasValue) detail.RequiresSignature = dto.RequiresSignature.Value;
            if (dto.Instructions != null) detail.Instructions = dto.Instructions;
            if (dto.IsActive.HasValue) detail.IsActive = dto.IsActive.Value;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated TaskDetail: {Id} - {DetailName}", id, detail.DetailName);

            return (true, "Cập nhật Task Detail thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating TaskDetail");
            return (false, $"Lỗi: {ex.Message}");
        }
    }

    /// <summary>
    /// Xóa Task Detail
    /// </summary>
    public async Task<(bool Success, string Message)> DeleteTaskDetailAsync(long id)
    {
        try
        {
            var detail = await _context.TaskDetails.FindAsync(id);
            if (detail == null)
            {
                return (false, "Không tìm thấy Task Detail");
            }

            // Kiểm tra xem có MaintenanceTaskDetail nào đang sử dụng không
            var inUse = await _context.MaintenanceTaskDetails.AnyAsync(mtd => mtd.TaskDetailId == id);
            if (inUse)
            {
                return (false, "Không thể xóa Task Detail đang được sử dụng");
            }

            detail.IsActive = false;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted TaskDetail: {Id} - {DetailName}", id, detail.DetailName);

            return (true, "Xóa Task Detail thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting TaskDetail");
            return (false, $"Lỗi: {ex.Message}");
        }
    }

    /// <summary>
    /// Reorder Task Details
    /// </summary>
    public async Task<(bool Success, string Message)> ReorderTaskDetailsAsync(int taskTypeId, ReorderTaskDetailsDto dto)
    {
        try
        {
            foreach (var item in dto.Items)
            {
                var detail = await _context.TaskDetails.FindAsync(item.TaskDetailId);
                if (detail != null && detail.TaskTypeId == taskTypeId)
                {
                    detail.OrderIndex = item.OrderIndex;
                }
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Reordered TaskDetails for TaskType: {TaskTypeId}", taskTypeId);

            return (true, "Sắp xếp lại thứ tự thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reordering TaskDetails");
            return (false, $"Lỗi: {ex.Message}");
        }
    }

    /// <summary>
    /// Lấy Task Type với tất cả Details
    /// </summary>
    public async Task<TaskTypeWithDetailsDto?> GetTaskTypeWithDetailsAsync(int taskTypeId)
    {
        var taskType = await GetTaskTypeByIdAsync(taskTypeId);
        if (taskType == null) return null;

        var details = await GetTaskDetailsByTypeIdAsync(taskTypeId, true);

        return new TaskTypeWithDetailsDto
        {
            TaskType = taskType,
            Details = details
        };
    }

    /// <summary>
    /// Thống kê Task Types theo category
    /// </summary>
    public async Task<Dictionary<string, int>> GetTaskTypeStatsByCategoryAsync()
    {
        return await _context.TaskTypes
            .Where(t => t.IsActive)
            .GroupBy(t => t.Category)
            .Select(g => new { Category = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Category, x => x.Count);
    }
}
