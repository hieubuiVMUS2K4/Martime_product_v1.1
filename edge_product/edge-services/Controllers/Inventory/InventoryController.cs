using MaritimeEdge.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace MaritimeEdge.Controllers.Inventory;

[ApiController]
[Route("api/inventory")]
public class InventoryController : ControllerBase
{
    private readonly EdgeDbContext _context;

    public InventoryController(EdgeDbContext context)
    {
        _context = context;
    }

    /// <summary>GET inventory stock (paged, filterable)</summary>
    [HttpGet]
    public async Task<ActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? storeLocationId = null,
        [FromQuery] string? q = null)
    {
        var query = from inv in _context.InventoryStocks
                    join mi in _context.MaterialItems on inv.MaterialItemId equals mi.Id
                    join sl in _context.StoreLocations on inv.StoreLocationId equals sl.Id
                    where mi.IsActive && sl.IsActive && inv.Quantity > 0
                    select new
                    {
                        inv.Id,
                        inv.MaterialItemId,
                        ItemCode = mi.ItemCode,
                        ItemName = mi.Name,
                        mi.Notes,
                        mi.Unit,
                        inv.StoreLocationId,
                        LocationName = sl.Name,
                        inv.Quantity,
                        inv.UnitCost,
                        TotalValue = inv.Quantity * inv.UnitCost,
                        inv.LastReceiptDate,
                        inv.UpdatedAt
                    };

        if (storeLocationId.HasValue)
            query = query.Where(x => x.StoreLocationId == storeLocationId.Value);

        if (!string.IsNullOrEmpty(q))
            query = query.Where(x =>
                x.ItemCode.Contains(q) ||
                x.ItemName.Contains(q) ||
                (x.Notes != null && x.Notes.Contains(q)));

        var total = await query.CountAsync();
        var totalValue = await query.SumAsync(x => x.TotalValue);

        var items = await query
            .OrderBy(x => x.ItemCode)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new { items, total, totalValue, page, pageSize });
    }

    /// <summary>GET summary: total items, total value, low-stock count</summary>
    [HttpGet("summary")]
    public async Task<ActionResult> GetSummary()
    {
        var totalItems = await _context.InventoryStocks.CountAsync(s => s.Quantity > 0);
        var totalValue = await _context.InventoryStocks.SumAsync(s => s.Quantity * s.UnitCost);

        var lowStockCount = await (
            from mi in _context.MaterialItems
            where mi.IsActive && mi.MinStock.HasValue && mi.OnHandQuantity < (double)mi.MinStock.Value
            select mi
        ).CountAsync();

        return Ok(new { totalItems, totalValue, lowStockCount });
    }

    /// <summary>GET stock by location (for tree panel)</summary>
    [HttpGet("by-location")]
    public async Task<ActionResult> GetByLocation()
    {
        var data = await (
            from inv in _context.InventoryStocks
            join sl in _context.StoreLocations on inv.StoreLocationId equals sl.Id
            where sl.IsActive && inv.Quantity > 0
            group inv by new { sl.Id, sl.Name, sl.ParentId } into g
            select new
            {
                LocationId = g.Key.Id,
                LocationName = g.Key.Name,
                ParentId = g.Key.ParentId,
                ItemCount = g.Count(),
                TotalValue = g.Sum(x => x.Quantity * x.UnitCost)
            }
        ).ToListAsync();

        return Ok(data);
    }

    /// <summary>Export inventory to CSV</summary>
    [HttpGet("export")]
    public async Task<ActionResult> Export([FromQuery] Guid? storeLocationId = null)
    {
        var items = await (from inv in _context.InventoryStocks
                    join mi in _context.MaterialItems on inv.MaterialItemId equals mi.Id
                    join sl in _context.StoreLocations on inv.StoreLocationId equals sl.Id
                    where mi.IsActive && sl.IsActive && inv.Quantity > 0
                        && (!storeLocationId.HasValue || inv.StoreLocationId == storeLocationId.Value)
                    orderby mi.ItemCode
                    select new {
                        mi.ItemCode,
                        ItemName = mi.Name,
                        mi.Notes,
                        LocationName = sl.Name,
                        inv.Quantity,
                        inv.UnitCost,
                        TotalValue = inv.Quantity * inv.UnitCost,
                        mi.Unit,
                        inv.UpdatedAt
                    }).ToListAsync();

        var sb = new StringBuilder();
        sb.AppendLine("Mã vật tư,Vật tư,Ghi chú,Vị trí kho,Số lượng tồn,Đơn giá (USD),Giá trị tồn (USD),ĐVT,Cập nhật");
        foreach (var r in items)
            sb.AppendLine($"\"{r.ItemCode}\",\"{r.ItemName}\",\"{r.Notes ?? ""}\",\"{r.LocationName}\",{r.Quantity},{r.UnitCost},{r.TotalValue},\"{r.Unit ?? ""}\",{r.UpdatedAt:yyyy-MM-dd}");

        return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", $"inventory-export-{DateTime.UtcNow:yyyyMMdd}.csv");
    }

    /// <summary>GET inventory history (stock changes from receipts & requests)</summary>
    [HttpGet("history")]
    public async Task<ActionResult> GetHistory(
        [FromQuery] Guid? storeLocationId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        // Stock receipts = IN
        var receipts = from sri in _context.StockReceiptItems
                       join sr in _context.StockReceipts on sri.ReceiptId equals sr.Id
                       join mi in _context.MaterialItems on sri.MaterialItemId equals mi.Id
                       where sri.MaterialItemId != null
                         && (!storeLocationId.HasValue || sri.StoreLocationId == storeLocationId.Value)
                       select new {
                           Date = sr.ReceivedDate,
                           Type = "IN",
                           mi.ItemCode,
                           ItemName = mi.Name,
                           Quantity = sri.QuantityReceived,
                           Note = sr.SupplierName ?? "",
                       };

        // Material requests = OUT
        var requests = from mri in _context.MaterialRequestItems
                       join mr in _context.MaterialRequests on mri.RequestId equals mr.Id
                       join mi in _context.MaterialItems on mri.MaterialItemId equals mi.Id
                       where mr.Status == "Approved" && mri.MaterialItemId != null
                       select new {
                           Date = mr.UpdatedAt,
                           Type = "OUT",
                           mi.ItemCode,
                           ItemName = mi.Name,
                           Quantity = mri.QuantityRequested,
                           Note = mr.Notes ?? "",
                       };

        var all = await receipts.Concat(requests)
            .OrderByDescending(x => x.Date)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var total = await receipts.Concat(requests).CountAsync();

        return Ok(new { items = all, total, page, pageSize });
    }

    /// <summary>Declare inventory (initial stock entry)</summary>
    [HttpPost("declare")]
    public async Task<ActionResult> Declare([FromBody] DeclareInventoryDto dto)
    {
        if (dto.Items == null || dto.Items.Count == 0)
            return BadRequest(new { error = "Không có dữ liệu khai báo" });

        foreach (var item in dto.Items)
        {
            var existing = await _context.InventoryStocks
                .FirstOrDefaultAsync(s => s.MaterialItemId == item.MaterialItemId && s.StoreLocationId == item.StoreLocationId);

            if (existing != null)
            {
                existing.Quantity = item.Quantity;
                existing.UnitCost = item.UnitCost;
                existing.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                _context.InventoryStocks.Add(new MaritimeEdge.Models.InventoryStock
                {
                    MaterialItemId = item.MaterialItemId,
                    StoreLocationId = item.StoreLocationId,
                    Quantity = item.Quantity,
                    UnitCost = item.UnitCost,
                    UpdatedAt = DateTime.UtcNow,
                });
            }

            // Also update MaterialItem.OnHandQuantity
            var mi = await _context.MaterialItems.FindAsync(item.MaterialItemId);
            if (mi != null)
            {
                var totalQty = await _context.InventoryStocks
                    .Where(s => s.MaterialItemId == item.MaterialItemId && s.StoreLocationId != item.StoreLocationId)
                    .SumAsync(s => (decimal?)s.Quantity) ?? 0;
                mi.OnHandQuantity = (double)(totalQty + item.Quantity);
            }
        }

        await _context.SaveChangesAsync();
        return Ok(new { success = true, count = dto.Items.Count });
    }

    /// <summary>Adjust inventory quantity</summary>
    [HttpPost("adjust")]
    public async Task<ActionResult> Adjust([FromBody] AdjustInventoryDto dto)
    {
        var stock = await _context.InventoryStocks
            .FirstOrDefaultAsync(s => s.MaterialItemId == dto.MaterialItemId && s.StoreLocationId == dto.StoreLocationId);

        if (stock == null)
            return NotFound(new { error = "Không tìm thấy tồn kho cho vật tư này tại vị trí đã chọn" });

        stock.Quantity += dto.AdjustQuantity;
        if (stock.Quantity < 0) stock.Quantity = 0;
        stock.UpdatedAt = DateTime.UtcNow;

        // Update MaterialItem.OnHandQuantity
        var mi = await _context.MaterialItems.FindAsync(dto.MaterialItemId);
        if (mi != null)
        {
            var totalQty = await _context.InventoryStocks
                .Where(s => s.MaterialItemId == dto.MaterialItemId)
                .SumAsync(s => (decimal?)s.Quantity) ?? 0;
            mi.OnHandQuantity = (double)totalQty;
        }

        await _context.SaveChangesAsync();
        return Ok(new { success = true, newQuantity = stock.Quantity });
    }
}

public class DeclareInventoryDto
{
    public List<DeclareItemDto> Items { get; set; } = new();
}

public class DeclareItemDto
{
    public Guid MaterialItemId { get; set; }
    public Guid StoreLocationId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
}

public class AdjustInventoryDto
{
    public Guid MaterialItemId { get; set; }
    public Guid StoreLocationId { get; set; }
    public decimal AdjustQuantity { get; set; }
    public string? Reason { get; set; }
}
