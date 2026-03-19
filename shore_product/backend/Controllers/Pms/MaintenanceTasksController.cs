using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductApi.Data;
using ProductApi.Models;

namespace ProductApi.Controllers.Pms;

/// <summary>
/// Read-only access to MaintenanceTasks synced from Edge vessels.
/// Tasks are created/updated on Edge and pushed to Shore via sync.
/// </summary>
[ApiController]
[Route("api/maintenance/tasks")]
public class MaintenanceTasksController : ControllerBase
{
    private readonly AppDbContext _context;

    public MaintenanceTasksController(AppDbContext context) => _context = context;

    /// <summary>
    /// GET /api/maintenance/tasks
    /// Query params: pageSize, page, status, priority, assignedTo, equipmentAssetId, 
    ///               equipmentGroupId, scheduleId, dateFrom, dateTo, isDeleted, originNode
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageSize = 1000,
        [FromQuery] int page = 1,
        [FromQuery] string? status = null,
        [FromQuery] string? priority = null,
        [FromQuery] string? assignedTo = null,
        [FromQuery] Guid? equipmentAssetId = null,
        [FromQuery] Guid? equipmentGroupId = null,
        [FromQuery] Guid? scheduleId = null,
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null,
        [FromQuery] string? originNode = null,
        [FromQuery] Guid? vesselId = null)
    {
        var query = _context.MaintenanceTasks
            .Where(t => !t.IsDeleted)
            .AsNoTracking();

        if (vesselId.HasValue)
            query = query.Where(t => t.VesselId == vesselId.Value);

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(t => t.Status == status.ToUpper());

        if (!string.IsNullOrWhiteSpace(priority))
            query = query.Where(t => t.Priority == priority.ToUpper());

        if (!string.IsNullOrWhiteSpace(assignedTo))
            query = query.Where(t => t.AssignedTo == assignedTo);

        if (equipmentAssetId.HasValue)
            query = query.Where(t => t.EquipmentAssetId == equipmentAssetId);

        if (equipmentGroupId.HasValue)
            query = query.Where(t => t.EquipmentGroupId == equipmentGroupId);

        if (scheduleId.HasValue)
            query = query.Where(t => t.ScheduleId == scheduleId);

        if (dateFrom.HasValue)
            query = query.Where(t => t.NextDueAt >= dateFrom.Value);

        if (dateTo.HasValue)
            query = query.Where(t => t.NextDueAt <= dateTo.Value);

        if (!string.IsNullOrWhiteSpace(originNode))
            query = query.Where(t => t.OriginNode == originNode);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(t => t.NextDueAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new
        {
            data = items,
            pagination = new
            {
                currentPage = page,
                pageSize,
                totalCount,
                totalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                hasNextPage = page * pageSize < totalCount,
                hasPreviousPage = page > 1,
            }
        });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var task = await _context.MaintenanceTasks
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);
        return task == null ? NotFound() : Ok(task);
    }

    /// <summary>
    /// GET /api/maintenance/tasks/stats
    /// Summary counts grouped by status for dashboard widgets.
    /// </summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats([FromQuery] string? originNode = null, [FromQuery] Guid? vesselId = null)
    {
        var query = _context.MaintenanceTasks.Where(t => !t.IsDeleted).AsNoTracking();
        if (vesselId.HasValue)
            query = query.Where(t => t.VesselId == vesselId.Value);
        if (!string.IsNullOrWhiteSpace(originNode))
            query = query.Where(t => t.OriginNode == originNode);

        var stats = await query
            .GroupBy(t => t.Status)
            .Select(g => new { status = g.Key, count = g.Count() })
            .ToListAsync();

        return Ok(stats);
    }
}
