using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductApi.Data;
using ProductApi.Models;

namespace ProductApi.Controllers.Materials;

[ApiController]
[Route("api/stock-receipts")]
public class StockReceiptsController : ControllerBase
{
    private readonly AppDbContext _context;

    public StockReceiptsController(AppDbContext context) => _context = context;

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null, [FromQuery] string? q = null)
    {
        var query = _context.StockReceipts.Where(r => r.IsActive).AsNoTracking();
        if (!string.IsNullOrEmpty(status)) query = query.Where(r => r.Status == status);
        if (!string.IsNullOrEmpty(q))
            query = query.Where(r => r.ReceiptCode.Contains(q) || (r.SupplierName != null && r.SupplierName.Contains(q)));
        var total = await query.CountAsync();
        var items = await query.Include(r => r.Items)
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return Ok(new { items, total, page, pageSize });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _context.StockReceipts.Include(r => r.Items)
            .AsNoTracking().FirstOrDefaultAsync(r => r.Id == id);
        return item == null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] StockReceipt dto)
    {
        var code = $"SR-{DateTime.UtcNow:yyyyMMdd}-{new Random().Next(1000, 9999)}";
        var entity = new StockReceipt
        {
            ReceiptCode = code, VesselName = dto.VesselName,
            SupplierCode = dto.SupplierCode, SupplierName = dto.SupplierName,
            ReceivedDate = dto.ReceivedDate, ReceiptDate = dto.ReceiptDate,
            CreatedBy = dto.CreatedBy, Notes = dto.Notes,
            MaterialRequestId = dto.MaterialRequestId,
        };
        foreach (var item in dto.Items ?? [])
            entity.Items.Add(new StockReceiptItem
            {
                StoreLocationId = item.StoreLocationId, MaterialItemId = item.MaterialItemId,
                ItemCode = item.ItemCode, ItemName = item.ItemName, Description = item.Description,
                Unit = item.Unit, QuantityRequested = item.QuantityRequested,
                QuantityReceived = item.QuantityReceived, UnitCost = item.UnitCost,
                Currency = item.Currency, Note = item.Note,
            });
        _context.StockReceipts.Add(entity);
        await _context.SaveChangesAsync();
        return Ok(new { id = entity.Id, receiptCode = entity.ReceiptCode });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] StockReceipt dto)
    {
        var entity = await _context.StockReceipts.AsTracking().Include(r => r.Items).FirstOrDefaultAsync(r => r.Id == id);
        if (entity == null) return NotFound();
        entity.VesselName = dto.VesselName; entity.SupplierCode = dto.SupplierCode;
        entity.SupplierName = dto.SupplierName; entity.ReceivedDate = dto.ReceivedDate;
        entity.Notes = dto.Notes; entity.MaterialRequestId = dto.MaterialRequestId;
        if (dto.Status != null) entity.Status = dto.Status;
        _context.StockReceiptItems.RemoveRange(entity.Items);
        foreach (var item in dto.Items ?? [])
            entity.Items.Add(new StockReceiptItem
            {
                StoreLocationId = item.StoreLocationId, MaterialItemId = item.MaterialItemId,
                ItemCode = item.ItemCode, ItemName = item.ItemName, Description = item.Description,
                Unit = item.Unit, QuantityRequested = item.QuantityRequested,
                QuantityReceived = item.QuantityReceived, UnitCost = item.UnitCost,
                Currency = item.Currency, Note = item.Note,
            });
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return Ok(entity);
    }

    [HttpPut("{id:int}/complete")]
    public async Task<IActionResult> Complete(int id)
    {
        var entity = await _context.StockReceipts.AsTracking().Include(r => r.Items).FirstOrDefaultAsync(r => r.Id == id);
        if (entity == null) return NotFound();

        // Update inventory stock
        foreach (var item in entity.Items.Where(i => i.MaterialItemId.HasValue && i.StoreLocationId.HasValue))
        {
            var stock = await _context.InventoryStocks.AsTracking().FirstOrDefaultAsync(
                s => s.MaterialItemId == item.MaterialItemId!.Value && s.StoreLocationId == item.StoreLocationId!.Value);
            if (stock == null)
            {
                _context.InventoryStocks.Add(new InventoryStock
                {
                    MaterialItemId = item.MaterialItemId!.Value,
                    StoreLocationId = item.StoreLocationId!.Value,
                    Quantity = item.QuantityReceived,
                    UnitCost = item.UnitCost ?? 0,
                    LastReceiptDate = DateTime.UtcNow,
                });
            }
            else
            {
                stock.Quantity += item.QuantityReceived;
                if (item.UnitCost.HasValue) stock.UnitCost = item.UnitCost.Value;
                stock.LastReceiptDate = DateTime.UtcNow;
                stock.UpdatedAt = DateTime.UtcNow;
            }

            // Update MaterialItem.OnHandQuantity
            var matItem = await _context.MaterialItems.FindAsync(item.MaterialItemId!.Value);
            if (matItem != null)
            {
                matItem.OnHandQuantity += (double)item.QuantityReceived;
                matItem.UpdatedAt = DateTime.UtcNow;
            }
        }

        entity.Status = "Completed"; entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return Ok(entity);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _context.StockReceipts.FindAsync(id);
        if (entity == null) return NotFound();
        entity.IsActive = false; entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
