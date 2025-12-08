using MaritimeEdge.Data;
using MaritimeEdge.DTOs;
using MaritimeEdge.Models;
using MaritimeEdge.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MaritimeEdge.Controllers;

[ApiController]
[Route("api/maintenance-schedules")]
public class MaintenanceScheduleController : ControllerBase
{
    private readonly IMaintenanceScheduleRepository _scheduleRepository;
    private readonly IEquipmentAssetRepository _assetRepository;
    private readonly EdgeDbContext _context;
    private readonly ILogger<MaintenanceScheduleController> _logger;

    public MaintenanceScheduleController(
        IMaintenanceScheduleRepository scheduleRepository,
        IEquipmentAssetRepository assetRepository,
        EdgeDbContext context,
        ILogger<MaintenanceScheduleController> logger)
    {
        _scheduleRepository = scheduleRepository;
        _assetRepository = assetRepository;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all maintenance schedules
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<MaintenanceScheduleDto>>> GetAll()
    {
        try
        {
            var schedules = await _scheduleRepository.GetAllAsync();
            var dtos = new List<MaintenanceScheduleDto>();

            foreach (var schedule in schedules)
            {
                var dto = await MapToDtoAsync(schedule);
                dtos.Add(dto);
            }

            return Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting maintenance schedules");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Get maintenance schedule by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<MaintenanceScheduleDto>> GetById(Guid id)
    {
        try
        {
            var schedule = await _scheduleRepository.GetByIdAsync(id);
            if (schedule == null)
                return NotFound(new { error = "Maintenance schedule not found" });

            var dto = await MapToDtoAsync(schedule);
            return Ok(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting maintenance schedule {Id}", id);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Get maintenance schedules by equipment group ID
    /// </summary>
    [HttpGet("group/{groupId}")]
    public async Task<ActionResult<List<MaintenanceScheduleDto>>> GetByGroupId(Guid groupId)
    {
        try
        {
            var schedules = await _scheduleRepository.GetByGroupIdAsync(groupId);
            var dtos = new List<MaintenanceScheduleDto>();

            foreach (var schedule in schedules)
            {
                var dto = await MapToDtoAsync(schedule);
                dtos.Add(dto);
            }

            return Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting maintenance schedules for group {GroupId}", groupId);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Create new maintenance schedule
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<MaintenanceScheduleDto>> Create([FromBody] CreateMaintenanceScheduleDto dto)
    {
        try
        {
            // Validate equipment group exists
            var group = await _context.EquipmentGroups.FindAsync(dto.EquipmentGroupId);
            if (group == null)
                return BadRequest(new { error = "Equipment group not found" });

            // Check if schedule code already exists
            if (await _scheduleRepository.ScheduleCodeExistsAsync(dto.ScheduleCode))
                return BadRequest(new { error = $"Schedule code '{dto.ScheduleCode}' already exists" });

            // Validate interval
            if (dto.IntervalType == "RUNNING_HOURS" && !dto.IntervalHours.HasValue)
                return BadRequest(new { error = "IntervalHours is required for RUNNING_HOURS interval type" });
            
            if (dto.IntervalType == "CALENDAR" && !dto.IntervalDays.HasValue)
                return BadRequest(new { error = "IntervalDays is required for CALENDAR interval type" });

            // Get first asset from group for next due date calculation
            var groupMembers = await _context.EquipmentGroupMembers
                .Where(egm => egm.GroupId == dto.EquipmentGroupId)
                .Include(egm => egm.Asset)
                .ToListAsync();
            
            var firstAsset = groupMembers.FirstOrDefault()?.Asset;
            if (firstAsset == null)
                return BadRequest(new { error = "Equipment group has no members" });

            var schedule = new MaintenanceSchedule
            {
                ScheduleCode = dto.ScheduleCode,
                EquipmentGroupId = dto.EquipmentGroupId,
                TaskTypeId = dto.TaskTypeId,
                ScheduleName = dto.ScheduleName,
                IntervalType = dto.IntervalType,
                IntervalHours = dto.IntervalHours,
                IntervalDays = dto.IntervalDays,
                DaysBeforeDue = dto.DaysBeforeDue,
                Priority = dto.Priority,
                EstimatedDurationHours = dto.EstimatedDurationHours,
                AutoGenerate = dto.AutoGenerate,
                Instructions = dto.Instructions,
                IsActive = true
            };

            // Calculate next due date
            CalculateNextDueDate(schedule, firstAsset);

            var created = await _scheduleRepository.CreateAsync(schedule);

            // Add spare parts if provided
            if (dto.RequiredSpareParts != null && dto.RequiredSpareParts.Count > 0)
            {
                var spareParts = dto.RequiredSpareParts.Select(sp => new ScheduleSparePart
                {
                    ScheduleId = created.Id,
                    MaterialItemId = sp.MaterialItemId,
                    QuantityRequired = sp.QuantityRequired,
                    IsMandatory = sp.IsMandatory,
                    Notes = sp.Notes
                }).ToList();

                await _scheduleRepository.AddSparePartsAsync(created.Id, spareParts);
            }

            // Add checklist templates if provided
            if (dto.ChecklistItemTemplates != null && dto.ChecklistItemTemplates.Count > 0)
            {
                var templates = dto.ChecklistItemTemplates.Select(t => new ScheduleChecklistTemplate
                {
                    ScheduleId = created.Id,
                    SequenceOrder = t.SequenceOrder,
                    CheckpointDescription = t.CheckpointDescription,
                    RequiresReading = t.RequiresReading,
                    NormalRangeMin = t.NormalRangeMin,
                    NormalRangeMax = t.NormalRangeMax,
                    Unit = t.Unit
                }).ToList();

                foreach (var template in templates)
                {
                    _context.ScheduleChecklistTemplates.Add(template);
                }
                await _context.SaveChangesAsync();
            }

            _logger.LogInformation("Created maintenance schedule {ScheduleCode}", created.ScheduleCode);

            var resultDto = await MapToDtoAsync(created);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, resultDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating maintenance schedule");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Preview next due dates for all schedules
    /// </summary>
    [HttpGet("preview")]
    public async Task<ActionResult<List<SchedulePreviewDto>>> PreviewDueDates()
    {
        try
        {
            var schedules = await _scheduleRepository.GetAllAsync();
            var previews = new List<SchedulePreviewDto>();

            foreach (var schedule in schedules)
            {
                var group = await _context.EquipmentGroups.FindAsync(schedule.EquipmentGroupId);
                if (group == null) continue;

                var daysUntilDue = schedule.NextDueDate.HasValue
                    ? (schedule.NextDueDate.Value.Date - DateTime.UtcNow.Date).Days
                    : 999;

                previews.Add(new SchedulePreviewDto
                {
                    ScheduleId = schedule.Id,
                    ScheduleName = schedule.ScheduleName,
                    AssetName = group.GroupName,
                    NextDueDate = schedule.NextDueDate,
                    NextDueRunningHours = schedule.NextDueRunningHours,
                    DaysUntilDue = daysUntilDue,
                    IsOverdue = daysUntilDue < 0,
                    Priority = schedule.Priority,
                    IntervalType = schedule.IntervalType,
                    IntervalValue = schedule.IntervalType == "RUNNING_HOURS" 
                        ? schedule.IntervalHours 
                        : schedule.IntervalDays,
                    EstimatedDurationHours = schedule.EstimatedDurationHours,
                    DaysBeforeDue = schedule.DaysBeforeDue
                });
            }

            return Ok(previews.OrderBy(p => p.DaysUntilDue).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error previewing due dates");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Update maintenance schedule
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<MaintenanceScheduleDto>> Update(Guid id, [FromBody] CreateMaintenanceScheduleDto dto)
    {
        try
        {
            var schedule = await _scheduleRepository.GetByIdAsync(id);
            if (schedule == null)
                return NotFound(new { error = "Maintenance schedule not found" });

            // Validate equipment group exists
            var group = await _context.EquipmentGroups.FindAsync(dto.EquipmentGroupId);
            if (group == null)
                return BadRequest(new { error = "Equipment group not found" });

            // Check if schedule code is being changed and if new code already exists
            if (schedule.ScheduleCode != dto.ScheduleCode && 
                await _scheduleRepository.ScheduleCodeExistsAsync(dto.ScheduleCode))
                return BadRequest(new { error = $"Schedule code '{dto.ScheduleCode}' already exists" });

            // Validate interval
            if (dto.IntervalType == "RUNNING_HOURS" && !dto.IntervalHours.HasValue)
                return BadRequest(new { error = "IntervalHours is required for RUNNING_HOURS interval type" });
            
            if (dto.IntervalType == "CALENDAR" && !dto.IntervalDays.HasValue)
                return BadRequest(new { error = "IntervalDays is required for CALENDAR interval type" });

            // Get first asset from group for next due date calculation
            var groupMembers = await _context.EquipmentGroupMembers
                .Where(egm => egm.GroupId == dto.EquipmentGroupId)
                .Include(egm => egm.Asset)
                .ToListAsync();
            
            var firstAsset = groupMembers.FirstOrDefault()?.Asset;
            if (firstAsset == null)
                return BadRequest(new { error = "Equipment group has no members" });

            // Update schedule fields
            schedule.ScheduleCode = dto.ScheduleCode;
            schedule.EquipmentGroupId = dto.EquipmentGroupId;
            schedule.TaskTypeId = dto.TaskTypeId;
            schedule.ScheduleName = dto.ScheduleName;
            schedule.IntervalType = dto.IntervalType;
            schedule.IntervalHours = dto.IntervalHours;
            schedule.IntervalDays = dto.IntervalDays;
            schedule.DaysBeforeDue = dto.DaysBeforeDue;
            schedule.Priority = dto.Priority;
            schedule.EstimatedDurationHours = dto.EstimatedDurationHours;
            schedule.AutoGenerate = dto.AutoGenerate;
            schedule.Instructions = dto.Instructions;
            schedule.UpdatedAt = DateTime.UtcNow;

            // Recalculate next due date
            CalculateNextDueDate(schedule, firstAsset);

            var updated = await _scheduleRepository.UpdateAsync(schedule);

            // Update spare parts - remove all and re-add
            await _scheduleRepository.RemoveSparePartsAsync(id);

            if (dto.RequiredSpareParts != null && dto.RequiredSpareParts.Count > 0)
            {
                var spareParts = dto.RequiredSpareParts.Select(sp => new ScheduleSparePart
                {
                    ScheduleId = id,
                    MaterialItemId = sp.MaterialItemId,
                    QuantityRequired = sp.QuantityRequired,
                    IsMandatory = sp.IsMandatory,
                    Notes = sp.Notes
                }).ToList();

                await _scheduleRepository.AddSparePartsAsync(id, spareParts);
            }

            _logger.LogInformation("Updated maintenance schedule {ScheduleCode}", updated.ScheduleCode);

            var resultDto = await MapToDtoAsync(updated);
            return Ok(resultDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating maintenance schedule {Id}", id);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Delete maintenance schedule
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        try
        {
            var schedule = await _scheduleRepository.GetByIdAsync(id);
            if (schedule == null)
                return NotFound(new { error = "Maintenance schedule not found" });

            await _scheduleRepository.DeleteAsync(id);

            _logger.LogInformation("Deleted maintenance schedule {ScheduleCode}", schedule.ScheduleCode);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting maintenance schedule {Id}", id);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    private async Task<MaintenanceScheduleDto> MapToDtoAsync(MaintenanceSchedule schedule)
    {
        var group = await _context.EquipmentGroups.FindAsync(schedule.EquipmentGroupId);
        var groupMembersCount = await _context.EquipmentGroupMembers
            .CountAsync(egm => egm.GroupId == schedule.EquipmentGroupId);
        var spareParts = await _scheduleRepository.GetSparePartsByScheduleIdAsync(schedule.Id);

        return new MaintenanceScheduleDto
        {
            Id = schedule.Id,
            ScheduleCode = schedule.ScheduleCode,
            EquipmentGroupId = schedule.EquipmentGroupId,
            GroupCode = group?.GroupCode,
            GroupName = group?.GroupName,
            AssetCount = groupMembersCount,
            TaskTypeId = schedule.TaskTypeId,
            ScheduleName = schedule.ScheduleName,
            IntervalType = schedule.IntervalType,
            IntervalHours = schedule.IntervalHours,
            IntervalDays = schedule.IntervalDays,
            DaysBeforeDue = schedule.DaysBeforeDue,
            LastExecutedAt = schedule.LastExecutedAt,
            LastExecutedRunningHours = schedule.LastExecutedRunningHours,
            NextDueDate = schedule.NextDueDate,
            NextDueRunningHours = schedule.NextDueRunningHours,
            Priority = schedule.Priority,
            EstimatedDurationHours = schedule.EstimatedDurationHours,
            AutoGenerate = schedule.AutoGenerate,
            Instructions = schedule.Instructions,
            IsActive = schedule.IsActive,
            RequiredSpareParts = spareParts.Select(sp => new ScheduleSparePartDto
            {
                Id = sp.Id,
                ScheduleId = sp.ScheduleId,
                MaterialItemId = sp.MaterialItemId,
                QuantityRequired = sp.QuantityRequired,
                IsMandatory = sp.IsMandatory,
                Notes = sp.Notes
            }).ToList()
        };
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
        }
        else if (schedule.IntervalType == "HYBRID")
        {
            // Calculate both calendar and running hours
            if (schedule.IntervalDays.HasValue)
            {
                var baseDate = schedule.LastExecutedAt ?? DateTime.UtcNow;
                schedule.NextDueDate = baseDate.AddDays(schedule.IntervalDays.Value);
            }
            
            if (schedule.IntervalHours.HasValue)
            {
                var baseHours = schedule.LastExecutedRunningHours ?? asset.CurrentRunningHours ?? 0;
                schedule.NextDueRunningHours = baseHours + schedule.IntervalHours.Value;
            }
        }
    }
}
