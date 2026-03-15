using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductApi.Data;
using ProductApi.Models;

namespace ProductApi.Controllers.Materials;

[ApiController]
[Route("api/inventory")]
public class InventoryController : ControllerBase
{
    private readonly AppDbContext _context;

    public InventoryController(AppDbContext context) => _context = context;

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 50,
        [FromQuery] string? storeLocationId = null, [FromQuery] string? q = null)
    {
        var stockQuery = _context.InventoryStocks.AsNoTracking();
        if (!string.IsNullOrEmpty(storeLocationId) && Guid.TryParse(storeLocationId, out var locId))
            stockQuery = stockQuery.Where(s => s.StoreLocationId == locId);

        var stocks = await stockQuery.ToListAsync();
        var matIds = stocks.Select(s => s.MaterialItemId).Distinct().ToList();
        var locIds = stocks.Select(s => s.StoreLocationId).Distinct().ToList();

        var materials = await _context.MaterialItems
            .Where(m => matIds.Contains(m.Id) && m.IsActive).AsNoTracking()
            .ToDictionaryAsync(m => m.Id);
        var locations = await _context.StoreLocations
            .Where(l => locIds.Contains(l.Id)).AsNoTracking()
            .ToDictionaryAsync(l => l.Id, l => l.Name);

        var items = stocks
            .Where(s => materials.ContainsKey(s.MaterialItemId))
            .Select(s =>
            {
                var m = materials[s.MaterialItemId];
                return new
                {
                    s.Id, s.MaterialItemId, s.StoreLocationId,
                    itemCode = m.ItemCode, itemName = m.Name, unit = m.Unit,
                    storeLocationName = locations.GetValueOrDefault(s.StoreLocationId, ""),
                    s.Quantity, s.UnitCost, totalValue = s.Quantity * s.UnitCost,
                    isLowStock = m.MinStock.HasValue && (double)s.Quantity <= m.MinStock.Value,
                    minStock = m.MinStock, s.LastReceiptDate
                };
            })
            .Where(x => string.IsNullOrEmpty(q) ||
                x.itemCode.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                x.itemName.Contains(q, StringComparison.OrdinalIgnoreCase))
            .OrderBy(x => x.itemCode)
            .ToList();

        var total = items.Count;
        var pagedItems = items.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        var totalValue = items.Sum(x => x.totalValue);
        return Ok(new { items = pagedItems, total, totalValue, page, pageSize });
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var stocks = await _context.InventoryStocks.AsNoTracking().ToListAsync();
        var matIds = stocks.Select(s => s.MaterialItemId).Distinct().ToList();
        var materials = await _context.MaterialItems
            .Where(m => matIds.Contains(m.Id)).AsNoTracking().ToListAsync();
        var matMap = materials.ToDictionary(m => m.Id);

        var totalItems = stocks.Count;
        var totalValue = stocks.Sum(s => s.Quantity * s.UnitCost);
        var lowStockCount = stocks.Count(s => matMap.ContainsKey(s.MaterialItemId)
            && matMap[s.MaterialItemId].MinStock.HasValue
            && (double)s.Quantity <= matMap[s.MaterialItemId].MinStock!.Value);

        return Ok(new { totalItems, totalValue, lowStockCount });
    }

    [HttpGet("by-location")]
    public async Task<IActionResult> GetByLocation()
    {
        var locations = await _context.StoreLocations.Where(l => l.IsActive).AsNoTracking().ToListAsync();
        var result = new List<object>();
        foreach (var loc in locations)
        {
            var stocks = await _context.InventoryStocks.Where(s => s.StoreLocationId == loc.Id).ToListAsync();
            result.Add(new
            {
                locationId = loc.Id.ToString(), locationName = loc.Name,
                parentId = loc.ParentId?.ToString(),
                itemCount = stocks.Count,
                totalValue = stocks.Sum(s => s.Quantity * s.UnitCost)
            });
        }
        return Ok(result);
    }

    [HttpGet("export")]
    public async Task<IActionResult> Export([FromQuery] string? storeLocationId)
    {
        // Returns JSON for CSV export (frontend handles actual CSV generation)
        var stockQuery = _context.InventoryStocks.AsNoTracking();
        if (!string.IsNullOrEmpty(storeLocationId) && Guid.TryParse(storeLocationId, out var locId))
            stockQuery = stockQuery.Where(s => s.StoreLocationId == locId);
        var stocks = await stockQuery.ToListAsync();
        return Ok(stocks);
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetHistory(
        [FromQuery] string? storeLocationId,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        // For now return completed stock receipts as history
        var query = _context.StockReceipts.Where(r => r.Status == "Completed").AsNoTracking();
        if (!string.IsNullOrEmpty(storeLocationId) && Guid.TryParse(storeLocationId, out var locId))
            query = query.Where(r => r.Items.Any(i => i.StoreLocationId == locId));
        var total = await query.CountAsync();
        var receipts = await query.Include(r => r.Items)
            .OrderByDescending(r => r.UpdatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        var items = receipts.SelectMany(r => r.Items.Select(i => new
        {
            date = r.UpdatedAt.ToString("yyyy-MM-dd"),
            type = "RECEIPT", itemCode = i.ItemCode ?? "", itemName = i.ItemName,
            quantity = (double)i.QuantityReceived, note = i.Note ?? ""
        })).ToList();
        return Ok(new { items, total, page, pageSize });
    }

    [HttpPost("declare")]
    public async Task<IActionResult> Declare([FromBody] DeclareRequest request)
    {
        foreach (var item in request.Items)
        {
            var stock = await _context.InventoryStocks.FirstOrDefaultAsync(
                s => s.MaterialItemId == item.MaterialItemId && s.StoreLocationId == item.StoreLocationId);
            if (stock == null)
            {
                _context.InventoryStocks.Add(new InventoryStock
                {
                    MaterialItemId = item.MaterialItemId, StoreLocationId = item.StoreLocationId,
                    Quantity = (decimal)item.Quantity, UnitCost = (decimal)item.UnitCost,
                    LastReceiptDate = DateTime.UtcNow,
                });
            }
            else
            {
                stock.Quantity = (decimal)item.Quantity;
                stock.UnitCost = (decimal)item.UnitCost;
                stock.UpdatedAt = DateTime.UtcNow;
            }
        }
        await _context.SaveChangesAsync();
        return Ok(new { success = true, count = request.Items.Count });
    }

    [HttpPost("adjust")]
    public async Task<IActionResult> Adjust([FromBody] AdjustRequest request)
    {
        var stock = await _context.InventoryStocks.FirstOrDefaultAsync(
            s => s.MaterialItemId == request.MaterialItemId && s.StoreLocationId == request.StoreLocationId);
        if (stock == null) return NotFound();
        stock.Quantity += (decimal)request.AdjustQuantity;
        if (stock.Quantity < 0) stock.Quantity = 0;
        stock.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return Ok(new { success = true, newQuantity = stock.Quantity });
    }

    public record DeclareItem(Guid MaterialItemId, Guid StoreLocationId, double Quantity, double UnitCost);
    public record DeclareRequest(List<DeclareItem> Items);
    public record AdjustRequest(Guid MaterialItemId, Guid StoreLocationId, double AdjustQuantity, string? Reason);
}
