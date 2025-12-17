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
    /// Get minimum lead time based on priority (ISM Code & IACS guidelines)
    /// CRITICAL: 30 days, HIGH: 14 days, MEDIUM: 10 days, LOW: 7 days
    /// </summary>
    private static int GetMinimumLeadTime(string priority)
    {
        return priority?.ToUpper() switch
        {
            "CRITICAL" => 30,
            "HIGH" => 14,
            "MEDIUM" => 10,
            "LOW" => 7,
            _ => 7
        };
    }

    /// <summary>
    /// Validate and auto-correct DaysBeforeDue based on ISM Code requirements
    /// For short intervals (≤ 7 days), use proportional lead time and ignore work duration
    /// </summary>
    private int ValidateAndCorrectLeadTime(int daysBeforeDue, string priority, double? estimatedHours, int? intervalDays = null)
    {
        var minimumLeadTime = GetMinimumLeadTime(priority);
        
        // For very short intervals (daily, hourly), use proportional lead time
        // This overrides both ISM Code minimums AND work duration calculations
        if (intervalDays.HasValue && intervalDays.Value <= 7)
        {
            // For short intervals: lead time = interval * 0.5 (minimum 1 day)
            var proportionalLeadTime = Math.Max(1, intervalDays.Value / 2);
            
            // For daily/weekly tasks, proportional lead time takes absolute priority
            // ALWAYS enforce proportional lead time - don't allow larger values
            if (daysBeforeDue != proportionalLeadTime)
            {
                _logger.LogWarning(
                    "DaysBeforeDue {Configured} adjusted to proportional {Minimum} for {Interval}-day interval. Short intervals require tight lead times.",
                    daysBeforeDue, proportionalLeadTime, intervalDays.Value);
            }
            return proportionalLeadTime;
        }
        
        // For longer intervals (> 7 days), use traditional ISM Code logic
        // Consider work duration (3x safety buffer)
        var workDays = (int)Math.Ceiling((estimatedHours ?? 4) / 8.0);
        var workBasedMinimum = workDays * 3;
        
        var effectiveMinimum = Math.Max(minimumLeadTime, workBasedMinimum);
        
        // 🚨 CEILING RULE: Lead time MUST NOT exceed interval to prevent task overlap
        // If interval exists, cap lead time at 70% of interval (best practice)
        // Example: 14-day interval -> max lead time = 9 days (not 30!)
        if (intervalDays.HasValue)
        {
            // Maximum lead time = 70% of interval (allows task to complete before next one appears)
            var maxAllowedLeadTime = (int)Math.Floor(intervalDays.Value * 0.7);
            
            // If effective minimum exceeds interval ceiling, use the ceiling
            if (effectiveMinimum > maxAllowedLeadTime)
            {
                _logger.LogWarning(
                    "ISM Code minimum {ISMMinimum} days for {Priority} priority exceeds interval ceiling {Ceiling} days (70% of {Interval}-day interval). " +
                    "Using ceiling to prevent task overlap.",
                    effectiveMinimum, priority, maxAllowedLeadTime, intervalDays.Value);
                effectiveMinimum = Math.Max(1, maxAllowedLeadTime); // Minimum 1 day
            }
            
            // ENFORCE CEILING: If configured lead time exceeds ceiling, reduce it
            if (daysBeforeDue > maxAllowedLeadTime)
            {
                _logger.LogWarning(
                    "DaysBeforeDue {Configured} exceeds interval ceiling {Ceiling} days (70% of {Interval}-day interval). " +
                    "Reducing to prevent task overlap.",
                    daysBeforeDue, maxAllowedLeadTime, intervalDays.Value);
                return Math.Max(1, maxAllowedLeadTime);
            }
        }
        
        if (daysBeforeDue < effectiveMinimum)
        {
            _logger.LogWarning(
                "DaysBeforeDue {Configured} is less than minimum {Minimum} for {Priority} priority. Auto-correcting.",
                daysBeforeDue, effectiveMinimum, priority);
            return effectiveMinimum;
        }
        
        return daysBeforeDue;
    }
    /// <summary>
    /// Get all maintenance schedules (OPTIMIZED - single query with includes)
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<MaintenanceScheduleDto>>> GetAll()
    {
        try
        {
            // Load all data in batch queries to avoid N+1
            var schedules = await _context.MaintenanceSchedules
                .AsNoTracking()
                .Where(s => s.IsActive)
                .OrderBy(s => s.NextDueDate)
                .ToListAsync();
            
            if (!schedules.Any())
                return Ok(new List<MaintenanceScheduleDto>());
            
            // Batch load all related data
            var groupIds = schedules.Select(s => s.EquipmentGroupId).Distinct().ToList();
            var scheduleIds = schedules.Select(s => s.Id).ToList();
            
            var groups = await _context.EquipmentGroups
                .AsNoTracking()
                .Where(g => groupIds.Contains(g.Id))
                .ToDictionaryAsync(g => g.Id);
            
            var memberCounts = await _context.EquipmentGroupMembers
                .AsNoTracking()
                .Where(egm => groupIds.Contains(egm.GroupId))
                .GroupBy(egm => egm.GroupId)
                .Select(g => new { GroupId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.GroupId, x => x.Count);
            
            var allSpareParts = await _context.ScheduleSpareParts
                .AsNoTracking()
                .Where(sp => scheduleIds.Contains(sp.ScheduleId))
                .ToListAsync();
            var sparePartsBySchedule = allSpareParts.GroupBy(sp => sp.ScheduleId)
                .ToDictionary(g => g.Key, g => g.ToList());
            
            var allChecklistTemplates = await _context.ScheduleChecklistTemplates
                .AsNoTracking()
                .Where(t => scheduleIds.Contains(t.ScheduleId))
                .OrderBy(t => t.SequenceOrder)
                .ToListAsync();
            var checklistsBySchedule = allChecklistTemplates.GroupBy(t => t.ScheduleId)
                .ToDictionary(g => g.Key, g => g.ToList());
            
            // Map to DTOs without additional queries
            var dtos = schedules.Select(schedule => {
                groups.TryGetValue(schedule.EquipmentGroupId, out var group);
                memberCounts.TryGetValue(schedule.EquipmentGroupId, out var memberCount);
                sparePartsBySchedule.TryGetValue(schedule.Id, out var spareParts);
                checklistsBySchedule.TryGetValue(schedule.Id, out var checklists);
                
                return new MaintenanceScheduleDto
                {
                    Id = schedule.Id,
                    ScheduleCode = schedule.ScheduleCode,
                    EquipmentGroupId = schedule.EquipmentGroupId,
                    GroupCode = group?.GroupCode,
                    GroupName = group?.GroupName,
                    AssetCount = memberCount,
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
                    RequiredSpareParts = spareParts?.Select(sp => new ScheduleSparePartDto
                    {
                        Id = sp.Id,
                        ScheduleId = sp.ScheduleId,
                        MaterialItemId = sp.MaterialItemId,
                        QuantityRequired = sp.QuantityRequired,
                        IsMandatory = sp.IsMandatory,
                        Notes = sp.Notes
                    }).ToList() ?? new List<ScheduleSparePartDto>(),
                    ChecklistItemTemplates = checklists?.Select(t => new ChecklistItemTemplateDto
                    {
                        SequenceOrder = t.SequenceOrder,
                        CheckpointDescription = t.CheckpointDescription,
                        RequiresReading = t.RequiresReading,
                        NormalRangeMin = t.NormalRangeMin,
                        NormalRangeMax = t.NormalRangeMax,
                        Unit = t.Unit
                    }).ToList() ?? new List<ChecklistItemTemplateDto>()
                };
            }).ToList();

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
            
            if (!schedules.Any())
                return Ok(new List<MaintenanceScheduleDto>());
            
            // Batch load related data (same optimization as GetAll)
            var scheduleIds = schedules.Select(s => s.Id).ToList();
            
            var group = await _context.EquipmentGroups.AsNoTracking()
                .FirstOrDefaultAsync(g => g.Id == groupId);
            
            var memberCount = await _context.EquipmentGroupMembers
                .AsNoTracking()
                .CountAsync(egm => egm.GroupId == groupId);
            
            var allSpareParts = await _context.ScheduleSpareParts
                .AsNoTracking()
                .Where(sp => scheduleIds.Contains(sp.ScheduleId))
                .ToListAsync();
            var sparePartsBySchedule = allSpareParts.GroupBy(sp => sp.ScheduleId)
                .ToDictionary(g => g.Key, g => g.ToList());
            
            var allChecklistTemplates = await _context.ScheduleChecklistTemplates
                .AsNoTracking()
                .Where(t => scheduleIds.Contains(t.ScheduleId))
                .OrderBy(t => t.SequenceOrder)
                .ToListAsync();
            var checklistsBySchedule = allChecklistTemplates.GroupBy(t => t.ScheduleId)
                .ToDictionary(g => g.Key, g => g.ToList());
            
            var dtos = schedules.Select(schedule => {
                sparePartsBySchedule.TryGetValue(schedule.Id, out var spareParts);
                checklistsBySchedule.TryGetValue(schedule.Id, out var checklists);
                
                return new MaintenanceScheduleDto
                {
                    Id = schedule.Id,
                    ScheduleCode = schedule.ScheduleCode,
                    EquipmentGroupId = schedule.EquipmentGroupId,
                    GroupCode = group?.GroupCode,
                    GroupName = group?.GroupName,
                    AssetCount = memberCount,
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
                    RequiredSpareParts = spareParts?.Select(sp => new ScheduleSparePartDto
                    {
                        Id = sp.Id,
                        ScheduleId = sp.ScheduleId,
                        MaterialItemId = sp.MaterialItemId,
                        QuantityRequired = sp.QuantityRequired,
                        IsMandatory = sp.IsMandatory,
                        Notes = sp.Notes
                    }).ToList() ?? new List<ScheduleSparePartDto>(),
                    ChecklistItemTemplates = checklists?.Select(t => new ChecklistItemTemplateDto
                    {
                        SequenceOrder = t.SequenceOrder,
                        CheckpointDescription = t.CheckpointDescription,
                        RequiresReading = t.RequiresReading,
                        NormalRangeMin = t.NormalRangeMin,
                        NormalRangeMax = t.NormalRangeMax,
                        Unit = t.Unit
                    }).ToList() ?? new List<ChecklistItemTemplateDto>()
                };
            }).ToList();

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
            // Log incoming request
            _logger.LogInformation("Creating schedule: Code={Code}, GroupId={GroupId}, Name={Name}, IntervalType={IntervalType}", 
                dto.ScheduleCode, dto.EquipmentGroupId, dto.ScheduleName, dto.IntervalType);

            // Validate model state
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                _logger.LogWarning("Model validation failed: {Errors}", string.Join(", ", errors));
                return BadRequest(new { error = "Validation failed", details = errors });
            }

            // Validate equipment group exists
            var group = await _context.EquipmentGroups.FindAsync(dto.EquipmentGroupId);
            if (group == null)
            {
                _logger.LogWarning("Equipment group not found: {GroupId}", dto.EquipmentGroupId);
                return BadRequest(new { error = "Equipment group not found" });
            }

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

            // ISM Code Compliance: Validate and auto-correct lead time based on priority
            // For RUNNING_HOURS: Convert hours to estimated days (÷ 10 hrs/day) for lead time validation
            var effectiveIntervalDays = dto.IntervalType == "RUNNING_HOURS" && dto.IntervalHours.HasValue
                ? (int)Math.Ceiling(dto.IntervalHours.Value / 10.0)
                : dto.IntervalDays;

            var validatedDaysBeforeDue = ValidateAndCorrectLeadTime(
                dto.DaysBeforeDue, 
                dto.Priority, 
                dto.EstimatedDurationHours,
                effectiveIntervalDays);

            var schedule = new MaintenanceSchedule
            {
                ScheduleCode = dto.ScheduleCode,
                EquipmentGroupId = dto.EquipmentGroupId,
                ScheduleName = dto.ScheduleName,
                IntervalType = dto.IntervalType,
                IntervalHours = dto.IntervalHours,
                IntervalDays = dto.IntervalDays,
                DaysBeforeDue = validatedDaysBeforeDue, // Use validated value
                Priority = dto.Priority,
                EstimatedDurationHours = dto.EstimatedDurationHours,
                AutoGenerate = dto.AutoGenerate,
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
    /// Calculate next due date based on interval type if not set
    /// </summary>
    private DateTime CalculateNextDueDateFromInterval(MaintenanceSchedule schedule)
    {
        var today = DateTime.UtcNow.Date;
        var lastExecuted = schedule.LastExecutedAt?.Date ?? today;
        
        var intervalDays = schedule.IntervalType?.ToUpper() switch
        {
            "DAILY" => 1,
            "WEEKLY" => 7,
            "BI_WEEKLY" or "BIWEEKLY" => 14,
            "MONTHLY" => 30,
            "QUARTERLY" => 90,
            "SEMI_ANNUALLY" or "SEMIANNUALLY" => 180,
            "ANNUALLY" => 365,
            "CALENDAR" => schedule.IntervalDays ?? 30,
            "RUNNING_HOURS" => schedule.IntervalHours.HasValue ? (int)Math.Ceiling(schedule.IntervalHours.Value / 12.0) : 30,
            _ => 30
        };
        
        return lastExecuted.AddDays(intervalDays);
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

                // Calculate next due date if not set
                var nextDueDate = schedule.NextDueDate ?? CalculateNextDueDateFromInterval(schedule);
                
                var daysUntilDue = (nextDueDate.Date - DateTime.UtcNow.Date).Days;

                previews.Add(new SchedulePreviewDto
                {
                    ScheduleId = schedule.Id,
                    ScheduleName = schedule.ScheduleName,
                    AssetName = group.GroupName,
                    NextDueDate = nextDueDate,
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

            // ISM Code Compliance: Validate and auto-correct lead time based on priority
            // For RUNNING_HOURS: Convert hours to estimated days (÷ 10 hrs/day) for lead time validation
            var effectiveIntervalDays = dto.IntervalType == "RUNNING_HOURS" && dto.IntervalHours.HasValue
                ? (int)Math.Ceiling(dto.IntervalHours.Value / 10.0)
                : dto.IntervalDays;

            var validatedDaysBeforeDue = ValidateAndCorrectLeadTime(
                dto.DaysBeforeDue, 
                dto.Priority, 
                dto.EstimatedDurationHours,
                effectiveIntervalDays);

            // Update schedule fields
            schedule.ScheduleCode = dto.ScheduleCode;
            schedule.EquipmentGroupId = dto.EquipmentGroupId;
            schedule.ScheduleName = dto.ScheduleName;
            schedule.IntervalType = dto.IntervalType;
            schedule.IntervalHours = dto.IntervalHours;
            schedule.IntervalDays = dto.IntervalDays;
            schedule.DaysBeforeDue = validatedDaysBeforeDue; // Use validated value
            schedule.Priority = dto.Priority;
            schedule.EstimatedDurationHours = dto.EstimatedDurationHours;
            schedule.AutoGenerate = dto.AutoGenerate;
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

            // Update checklist templates - remove all and re-add
            var existingTemplates = await _context.ScheduleChecklistTemplates
                .Where(t => t.ScheduleId == id)
                .ToListAsync();
            
            _context.ScheduleChecklistTemplates.RemoveRange(existingTemplates);
            await _context.SaveChangesAsync();

            if (dto.ChecklistItemTemplates != null && dto.ChecklistItemTemplates.Count > 0)
            {
                var templates = dto.ChecklistItemTemplates.Select(t => new ScheduleChecklistTemplate
                {
                    ScheduleId = id,
                    SequenceOrder = t.SequenceOrder,
                    CheckpointDescription = t.CheckpointDescription,
                    RequiresReading = t.RequiresReading,
                    NormalRangeMin = t.NormalRangeMin,
                    NormalRangeMax = t.NormalRangeMax,
                    Unit = t.Unit,
                    CreatedAt = DateTime.UtcNow
                }).ToList();

                _context.ScheduleChecklistTemplates.AddRange(templates);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Updated {Count} checklist templates for schedule {ScheduleCode}", 
                    templates.Count, updated.ScheduleCode);
            }

            // AUTO-UPDATE EXISTING TASKS: Sync checklist changes to active tasks
            await SyncChecklistToActiveTasks(id, updated.ScheduleCode);

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
    /// Delete maintenance schedule and all its generated tasks
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        try
        {
            var schedule = await _scheduleRepository.GetByIdAsync(id);
            if (schedule == null)
                return NotFound(new { error = "Maintenance schedule not found" });

            // ⚠️ IMPORTANT: Delete all tasks generated from this schedule FIRST
            // (to avoid orphaned tasks showing up in Kanban after schedule deletion)
            var tasksToDelete = await _context.MaintenanceTasks
                .Where(t => t.ScheduleId == schedule.Id)
                .ToListAsync();
            
            if (tasksToDelete.Any())
            {
                _logger.LogWarning(
                    "Deleting {Count} tasks generated from schedule {ScheduleCode}", 
                    tasksToDelete.Count, 
                    schedule.ScheduleCode);
                
                _context.MaintenanceTasks.RemoveRange(tasksToDelete);
                await _context.SaveChangesAsync();
            }

            // Then delete the schedule
            await _scheduleRepository.DeleteAsync(id);

            _logger.LogInformation(
                "Deleted maintenance schedule {ScheduleCode} and {TaskCount} associated tasks", 
                schedule.ScheduleCode, 
                tasksToDelete.Count);

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
        var checklistTemplates = await _context.ScheduleChecklistTemplates
            .Where(t => t.ScheduleId == schedule.Id)
            .OrderBy(t => t.SequenceOrder)
            .ToListAsync();

        return new MaintenanceScheduleDto
        {
            Id = schedule.Id,
            ScheduleCode = schedule.ScheduleCode,
            EquipmentGroupId = schedule.EquipmentGroupId,
            GroupCode = group?.GroupCode,
            GroupName = group?.GroupName,
            AssetCount = groupMembersCount,
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
            }).ToList(),
            ChecklistItemTemplates = checklistTemplates.Select(t => new ChecklistItemTemplateDto
            {
                SequenceOrder = t.SequenceOrder,
                CheckpointDescription = t.CheckpointDescription,
                RequiresReading = t.RequiresReading,
                NormalRangeMin = t.NormalRangeMin,
                NormalRangeMax = t.NormalRangeMax,
                Unit = t.Unit
            }).ToList()
        };
    }

    /// <summary>
    /// Sync checklist template changes to active tasks (not IN_PROGRESS or COMPLETED)
    /// </summary>
    private async Task SyncChecklistToActiveTasks(Guid scheduleId, string scheduleCode)
    {
        try
        {
            // Find all active tasks for this schedule (TASK, MISSING_*, PENDING_APPROVAL, PENDING)
            var activeTasks = await _context.MaintenanceTasks
                .Where(t => !t.IsDeleted && 
                           t.TaskId.StartsWith($"SCHED-{scheduleCode}") &&
                           t.Status != "IN_PROGRESS" &&
                           t.Status != "COMPLETED" &&
                           t.Status != "CANCELLED")
                .ToListAsync();

            if (!activeTasks.Any())
            {
                _logger.LogDebug("No active tasks found for schedule {ScheduleCode}", scheduleCode);
                return;
            }

            // Get updated templates
            var newTemplates = await _context.ScheduleChecklistTemplates
                .Where(t => t.ScheduleId == scheduleId)
                .OrderBy(t => t.SequenceOrder)
                .ToListAsync();

            int tasksUpdated = 0;
            int checklistItemsAdded = 0;

            foreach (var task in activeTasks)
            {
                // Get equipment group to find assets
                var groupMembers = await _context.EquipmentGroupMembers
                    .Where(egm => egm.GroupId == task.EquipmentGroupId)
                    .Include(egm => egm.Asset)
                    .Where(egm => egm.Asset != null && egm.Asset.IsActive)
                    .ToListAsync();

                if (!groupMembers.Any()) continue;

                // Remove existing checklist items for this task
                var existingItems = await _context.TaskChecklistItems
                    .Where(ci => ci.TaskId == task.TaskId)
                    .ToListAsync();
                
                _context.TaskChecklistItems.RemoveRange(existingItems);

                // Add new checklist items based on updated templates
                if (newTemplates.Any())
                {
                    foreach (var member in groupMembers)
                    {
                        var asset = member.Asset;
                        if (asset == null) continue;

                        foreach (var template in newTemplates)
                        {
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
                            _context.TaskChecklistItems.Add(checklistItem);
                            checklistItemsAdded++;
                        }
                    }
                }

                // UPDATE TASK STATUS based on new validation
                var hasPIC = !string.IsNullOrWhiteSpace(task.AssignedTo);
                var hasChecklist = newTemplates.Any();
                var oldStatus = task.Status;
                var newStatus = DetermineTaskStatus(task.AssignedTo, hasChecklist, task.Priority);

                if (oldStatus != newStatus)
                {
                    task.Status = newStatus;
                    task.UpdatedAt = DateTime.UtcNow;
                    _logger.LogInformation(
                        "Task {TaskId} status updated: {OldStatus} → {NewStatus} (PIC: {HasPIC}, Checklist: {HasChecklist})",
                        task.TaskId, oldStatus, newStatus, hasPIC, hasChecklist);
                }

                tasksUpdated++;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Synced checklist changes to {TaskCount} active tasks: {ItemCount} checklist items updated for schedule {ScheduleCode}",
                tasksUpdated, checklistItemsAdded, scheduleCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing checklist to active tasks for schedule {ScheduleCode}", scheduleCode);
            // Don't throw - this is a background sync operation
        }
    }

    /// <summary>
    /// Determine task status based on validation rules (shared with scheduler)
    /// </summary>
    private string DetermineTaskStatus(string? assignedTo, bool hasChecklist, string priority)
    {
        bool hasPIC = !string.IsNullOrWhiteSpace(assignedTo);

        if (!hasPIC && !hasChecklist) return "MISSING_BOTH";
        if (!hasPIC) return "MISSING_PIC";
        if (!hasChecklist) return "MISSING_CHECKLIST";

        return (priority == "HIGH" || priority == "CRITICAL") ? "PENDING_APPROVAL" : "PENDING";
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
}

