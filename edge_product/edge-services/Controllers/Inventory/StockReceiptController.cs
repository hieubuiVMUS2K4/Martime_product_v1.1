using MaritimeEdge.Models;
using MaritimeEdge.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MaritimeEdge.Controllers.Inventory;

// ========== DTOs ==========
public class CreateStockReceiptDto
{
    public string? SupplierCode { get; set; }
    public string? SupplierName { get; set; }
    public DateTime ReceivedDate { get; set; }
    public DateTime ReceiptDate { get; set; }
    public string? CreatedBy { get; set; }
    public string? Notes { get; set; }
    public int? MaterialRequestId { get; set; }
    public List<CreateStockReceiptItemDto> Items { get; set; } = new();
}

public class CreateStockReceiptItemDto
{
    public Guid? StoreLocationId { get; set; }
    public Guid? MaterialItemId { get; set; }
    public string? ItemCode { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Unit { get; set; } = "PCS";
    public decimal QuantityRequested { get; set; }
    public decimal QuantityReceived { get; set; }
    public decimal? UnitCost { get; set; }
    public string? Currency { get; set; } = "USD";
    public string? Note { get; set; }
}

public class UpdateStockReceiptDto
{
    public string? SupplierCode { get; set; }
    public string? SupplierName { get; set; }
    public DateTime? ReceivedDate { get; set; }
    public DateTime? ReceiptDate { get; set; }
    public string? Notes { get; set; }
    public string? Status { get; set; }
    public List<CreateStockReceiptItemDto>? Items { get; set; }
}

[ApiController]
[Route("api/stock-receipts")]
public class StockReceiptController : ControllerBase
{
    private readonly EdgeDbContext _context;

    public StockReceiptController(EdgeDbContext context)
    {
        _context = context;
    }

    /// <summary>GET all receipts (paged)</summary>
    [HttpGet]
    public async Task<ActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? status = null,
        [FromQuery] string? q = null)
    {
        var query = _context.StockReceipts
            .Where(r => r.IsActive)
            .AsQueryable();

        if (!string.IsNullOrEmpty(status))
            query = query.Where(r => r.Status == status);

        if (!string.IsNullOrEmpty(q))
            query = query.Where(r =>
                r.ReceiptCode.Contains(q) ||
                (r.SupplierName != null && r.SupplierName.Contains(q)) ||
                (r.Notes != null && r.Notes.Contains(q)));

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new
            {
                r.Id,
                r.ReceiptCode,
                r.SupplierCode,
                r.SupplierName,
                r.ReceivedDate,
                r.ReceiptDate,
                r.CreatedBy,
                r.Notes,
                r.Status,
                r.MaterialRequestId,
                RequestCode = r.MaterialRequest != null ? r.MaterialRequest.RequestCode : null,
                r.CreatedAt,
                r.UpdatedAt,
                ItemCount = r.Items.Count,
                TotalValue = r.Items.Sum(i => (i.QuantityReceived * (i.UnitCost ?? 0)))
            })
            .ToListAsync();

        return Ok(new { items, total, page, pageSize });
    }

    /// <summary>GET single receipt with items</summary>
    [HttpGet("{id}")]
    public async Task<ActionResult> GetById(int id)
    {
        var receipt = await _context.StockReceipts
            .Where(r => r.Id == id && r.IsActive)
            .Select(r => new
            {
                r.Id,
                r.ReceiptCode,
                r.SupplierCode,
                r.SupplierName,
                r.ReceivedDate,
                r.ReceiptDate,
                r.CreatedBy,
                r.Notes,
                r.Status,
                r.MaterialRequestId,
                RequestCode = r.MaterialRequest != null ? r.MaterialRequest.RequestCode : null,
                r.CreatedAt,
                r.UpdatedAt,
                Items = r.Items.Select(i => new
                {
                    i.Id,
                    i.StoreLocationId,
                    i.MaterialItemId,
                    i.ItemCode,
                    i.ItemName,
                    i.Description,
                    i.Unit,
                    i.QuantityRequested,
                    i.QuantityReceived,
                    i.UnitCost,
                    i.Currency,
                    i.Note
                }).ToList()
            })
            .FirstOrDefaultAsync();

        if (receipt == null) return NotFound();
        return Ok(receipt);
    }

    /// <summary>POST create new receipt (Draft)</summary>
    [HttpPost]
    public async Task<ActionResult> Create([FromBody] CreateStockReceiptDto dto)
    {
        // Generate code: NK-YYYYMMDD-XXX
        var today = DateTime.UtcNow.ToString("yyyyMMdd");
        var countToday = await _context.StockReceipts
            .CountAsync(r => r.ReceiptCode.StartsWith($"NK-{today}"));
        var code = $"NK-{today}-{(countToday + 1):D3}";

        var receipt = new StockReceipt
        {
            ReceiptCode = code,
            SupplierCode = dto.SupplierCode,
            SupplierName = dto.SupplierName,
            ReceivedDate = DateTime.SpecifyKind(dto.ReceivedDate, DateTimeKind.Utc),
            ReceiptDate = DateTime.SpecifyKind(dto.ReceiptDate, DateTimeKind.Utc),
            CreatedBy = dto.CreatedBy,
            Notes = dto.Notes,
            MaterialRequestId = dto.MaterialRequestId,
            Status = "Draft"
        };

        foreach (var item in dto.Items)
        {
            receipt.Items.Add(new StockReceiptItem
            {
                StoreLocationId = item.StoreLocationId,
                MaterialItemId = item.MaterialItemId,
                ItemCode = item.ItemCode,
                ItemName = item.ItemName,
                Description = item.Description,
                Unit = item.Unit,
                QuantityRequested = item.QuantityRequested,
                QuantityReceived = item.QuantityReceived,
                UnitCost = item.UnitCost,
                Currency = item.Currency,
                Note = item.Note
            });
        }

        _context.StockReceipts.Add(receipt);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = receipt.Id }, new { receipt.Id, receipt.ReceiptCode });
    }

    /// <summary>PUT update receipt</summary>
    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, [FromBody] UpdateStockReceiptDto dto)
    {
        var receipt = await _context.StockReceipts
            .Include(r => r.Items)
            .FirstOrDefaultAsync(r => r.Id == id && r.IsActive);

        if (receipt == null) return NotFound();

        if (dto.SupplierCode != null) receipt.SupplierCode = dto.SupplierCode;
        if (dto.SupplierName != null) receipt.SupplierName = dto.SupplierName;
        if (dto.ReceivedDate.HasValue) receipt.ReceivedDate = DateTime.SpecifyKind(dto.ReceivedDate.Value, DateTimeKind.Utc);
        if (dto.ReceiptDate.HasValue) receipt.ReceiptDate = DateTime.SpecifyKind(dto.ReceiptDate.Value, DateTimeKind.Utc);
        if (dto.Notes != null) receipt.Notes = dto.Notes;
        if (dto.Status != null) receipt.Status = dto.Status;
        receipt.UpdatedAt = DateTime.UtcNow;

        if (dto.Items != null)
        {
            _context.StockReceiptItems.RemoveRange(receipt.Items);
            foreach (var item in dto.Items)
            {
                receipt.Items.Add(new StockReceiptItem
                {
                    StoreLocationId = item.StoreLocationId,
                    MaterialItemId = item.MaterialItemId,
                    ItemCode = item.ItemCode,
                    ItemName = item.ItemName,
                    Description = item.Description,
                    Unit = item.Unit,
                    QuantityRequested = item.QuantityRequested,
                    QuantityReceived = item.QuantityReceived,
                    UnitCost = item.UnitCost,
                    Currency = item.Currency,
                    Note = item.Note
                });
            }
        }

        await _context.SaveChangesAsync();
        return Ok(new { receipt.Id, receipt.ReceiptCode, receipt.Status });
    }

    /// <summary>PUT complete receipt → update inventory_stock</summary>
    [HttpPut("{id}/complete")]
    public async Task<ActionResult> Complete(int id)
    {
        var receipt = await _context.StockReceipts
            .Include(r => r.Items)
            .FirstOrDefaultAsync(r => r.Id == id && r.IsActive);

        if (receipt == null) return NotFound();
        if (receipt.Status == "Completed") return BadRequest("Already completed.");

        // Update inventory_stock for each item
        foreach (var item in receipt.Items)
        {
            if (item.MaterialItemId == null || item.StoreLocationId == null) continue;

            var stock = await _context.InventoryStocks
                .FirstOrDefaultAsync(s =>
                    s.MaterialItemId == item.MaterialItemId.Value &&
                    s.StoreLocationId == item.StoreLocationId.Value);

            if (stock != null)
            {
                stock.Quantity += item.QuantityReceived;
                if (item.UnitCost.HasValue) stock.UnitCost = item.UnitCost.Value;
                stock.LastReceiptDate = DateTime.UtcNow;
                stock.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                _context.InventoryStocks.Add(new InventoryStock
                {
                    MaterialItemId = item.MaterialItemId.Value,
                    StoreLocationId = item.StoreLocationId.Value,
                    Quantity = item.QuantityReceived,
                    UnitCost = item.UnitCost ?? 0,
                    LastReceiptDate = DateTime.UtcNow
                });
            }

            // Also update MaterialItem.OnHandQuantity
            var materialItem = await _context.MaterialItems.FindAsync(item.MaterialItemId.Value);
            if (materialItem != null)
            {
                materialItem.OnHandQuantity += (double)item.QuantityReceived;
                materialItem.UpdatedAt = DateTime.UtcNow;
            }
        }

        receipt.Status = "Completed";
        receipt.UpdatedAt = DateTime.UtcNow;

        // If linked to a request, mark it as Completed
        if (receipt.MaterialRequestId.HasValue)
        {
            var request = await _context.MaterialRequests.FindAsync(receipt.MaterialRequestId.Value);
            if (request != null)
            {
                request.Status = "Completed";
                request.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync();
        return Ok(new { receipt.Id, receipt.Status });
    }

    /// <summary>DELETE soft-delete</summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var receipt = await _context.StockReceipts.FindAsync(id);
        if (receipt == null || !receipt.IsActive) return NotFound();

        receipt.IsActive = false;
        receipt.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
