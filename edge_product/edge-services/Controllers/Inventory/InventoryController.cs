using MaritimeEdge.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
}
