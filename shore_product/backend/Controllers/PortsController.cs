using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductApi.Data;
using ProductApi.Models;
using ProductApi.Services.Sync;
using Maritime.Shared.Models.Sync;

namespace ProductApi.Controllers;

public class PortPayload
{
    public string PortCode { get; set; } = string.Empty;
    public string PortName { get; set; } = string.Empty;
    public string? Country { get; set; }
    public string? CountryCode { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? TimeZone { get; set; }
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Danh mục cảng trên bờ — dùng chung cho mọi tàu.
///
/// Bờ làm chủ danh mục này: thêm/sửa ở đây rồi phát xuống tất cả các tàu, giống cách
/// làm với chức danh, quốc gia và loại chứng chỉ. Tàu nhận về nhưng không sửa ngược.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PortsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ISyncOutboxService _syncOutbox;
    private readonly ILogger<PortsController> _logger;

    public PortsController(AppDbContext context, ISyncOutboxService syncOutbox, ILogger<PortsController> logger)
    {
        _context = context;
        _syncOutbox = syncOutbox;
        _logger = logger;
    }

    /// <summary>
    /// GET /api/ports — tìm theo tên cảng, mã UN/LOCODE hoặc quốc gia.
    /// Mặc định chỉ trả cảng đang dùng; truyền isActive=false để xem cảng đã ngừng.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetPorts(
        [FromQuery] string? search,
        [FromQuery] string? countryCode,
        [FromQuery] bool? isActive,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        try
        {
            var q = _context.Ports.AsNoTracking().AsQueryable();

            // Không truyền isActive = chỉ cảng đang dùng (giữ nguyên hành vi cũ)
            q = isActive.HasValue ? q.Where(p => p.IsActive == isActive.Value) : q.Where(p => p.IsActive);

            if (!string.IsNullOrWhiteSpace(countryCode))
                q = q.Where(p => p.CountryCode == countryCode.ToUpper());

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.ToLower();
                q = q.Where(p =>
                    p.PortCode.ToLower().Contains(s) ||
                    p.PortName.ToLower().Contains(s) ||
                    (p.Country != null && p.Country.ToLower().Contains(s)));
            }

            if (page < 1) page = 1;
            if (pageSize <= 0) pageSize = 50;
            if (pageSize > 500) pageSize = 500;

            var total = await q.CountAsync();

            var ports = await q
                .OrderBy(p => p.Country)
                .ThenBy(p => p.PortName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new
                {
                    p.Id,
                    p.PortCode,
                    p.PortName,
                    p.Country,
                    p.CountryCode,
                    p.Latitude,
                    p.Longitude,
                    p.TimeZone,
                    p.IsActive,
                })
                .ToListAsync();

            // `total` giữ lại cho caller cũ (port.service.ts), `pagination` cho màn hình danh mục
            return Ok(new
            {
                data = ports,
                total,
                pagination = new
                {
                    currentPage = page,
                    pageSize,
                    totalCount = total,
                    totalPages = (int)Math.Ceiling(total / (double)pageSize),
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing ports");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetPort(int id)
    {
        var port = await _context.Ports.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
        return port == null ? NotFound(new { error = "Không tìm thấy cảng" }) : Ok(port);
    }

    /// <summary>POST /api/ports — thêm cảng mới và phát xuống mọi tàu.</summary>
    [HttpPost]
    public async Task<IActionResult> CreatePort([FromBody] PortPayload payload)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(payload.PortCode) || string.IsNullOrWhiteSpace(payload.PortName))
                return BadRequest(new { error = "Mã cảng và tên cảng là bắt buộc" });

            var code = payload.PortCode.Trim().ToUpper();
            if (await _context.Ports.AnyAsync(p => p.PortCode == code))
                return BadRequest(new { error = $"Mã cảng '{code}' đã tồn tại" });

            var port = new Port
            {
                PortCode = code,
                PortName = payload.PortName.Trim(),
                Country = payload.Country,
                CountryCode = payload.CountryCode?.ToUpper(),
                Latitude = payload.Latitude,
                Longitude = payload.Longitude,
                TimeZone = payload.TimeZone,
                IsActive = payload.IsActive,
                OriginNode = "SHORE",
                IsSynced = false,
            };

            _context.Ports.Add(port);
            await _context.SaveChangesAsync();
            await BroadcastAsync(port, SyncActionType.CREATE);

            return CreatedAtAction(nameof(GetPort), new { id = port.Id }, port);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating port");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdatePort(int id, [FromBody] PortPayload payload)
    {
        try
        {
            var port = await _context.Ports.FirstOrDefaultAsync(p => p.Id == id);
            if (port == null) return NotFound(new { error = "Không tìm thấy cảng" });

            if (!string.IsNullOrWhiteSpace(payload.PortCode))
            {
                var code = payload.PortCode.Trim().ToUpper();
                if (code != port.PortCode && await _context.Ports.AnyAsync(p => p.PortCode == code))
                    return BadRequest(new { error = $"Mã cảng '{code}' đã tồn tại" });
                port.PortCode = code;
            }

            if (!string.IsNullOrWhiteSpace(payload.PortName)) port.PortName = payload.PortName.Trim();
            port.Country = payload.Country;
            port.CountryCode = payload.CountryCode?.ToUpper();
            port.Latitude = payload.Latitude;
            port.Longitude = payload.Longitude;
            port.TimeZone = payload.TimeZone;
            port.IsActive = payload.IsActive;
            port.IsSynced = false;
            port.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await BroadcastAsync(port, SyncActionType.UPDATE);

            return Ok(port);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating port {Id}", id);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// DELETE /api/ports/{id} — ngừng sử dụng (đặt IsActive = false).
    /// KHÔNG xoá cứng: các chuyến đi và mục sổ thuyền viên cũ còn tham chiếu tới cảng.
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeactivatePort(int id)
    {
        try
        {
            var port = await _context.Ports.FirstOrDefaultAsync(p => p.Id == id);
            if (port == null) return NotFound(new { error = "Không tìm thấy cảng" });

            port.IsActive = false;
            port.IsSynced = false;
            port.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            await BroadcastAsync(port, SyncActionType.UPDATE);

            return Ok(new { message = "Đã ngừng sử dụng cảng" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating port {Id}", id);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    private async Task BroadcastAsync(Port port, SyncActionType action)
    {
        try
        {
            await _syncOutbox.BroadcastAsync("port", port.Id.ToString(), action, port);
        }
        catch (Exception ex)
        {
            // Không chặn thao tác trên bờ chỉ vì đẩy xuống tàu thất bại
            _logger.LogWarning(ex, "Không phát được cảng {Code} xuống các tàu", port.PortCode);
        }
    }
}
