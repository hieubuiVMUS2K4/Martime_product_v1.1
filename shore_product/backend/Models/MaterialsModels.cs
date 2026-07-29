using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductApi.Models;

// ============================================================
// MATERIALS — CATEGORIES
// ============================================================

public class MaterialCategory
{
    [Key]
    public long Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string CategoryCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public long? ParentCategoryId { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

// ============================================================
// MATERIALS — CATALOG ITEMS (fleet-wide shared "danh mục vật tư")
// Bảng danh mục dùng chung cho tất cả các tàu của công ty.
// Chỉ chứa thông tin định danh vật tư: mã, tên, loại, đơn giá.
// ============================================================

public class MaterialItem
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Mã vật tư (dùng chung toàn fleet, duy nhất).</summary>
    [Required]
    [MaxLength(50)]
    public string ItemCode { get; set; } = string.Empty;

    /// <summary>Tên vật tư.</summary>
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Mã loại vật tư (FK → material_categories).</summary>
    public long CategoryId { get; set; }

    /// <summary>Đơn giá tham chiếu của vật tư.</summary>
    public decimal? UnitPrice { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public virtual ICollection<MaterialItemShip> ShipItems { get; set; } = new List<MaterialItemShip>();
}

// ============================================================
// MATERIALS — SHIP ITEMS (vật tư theo từng tàu / kho trên tàu)
// Bảng cũ material_items đổi tên thành material_item_ship.
// Giữ số lượng, min/max, vị trí kho, VesselId... theo từng tàu.
// ============================================================

public class MaterialItemShip
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Mã vật tư trên tàu (mã nội bộ của tàu, trước đây là ItemCode).</summary>
    [Required]
    [MaxLength(50)]
    public string ShipItemCode { get; set; } = string.Empty;

    /// <summary>Mã vật tư — FK trỏ về danh mục chung material_items.ItemCode (trước đây là cột Name).</summary>
    [Required]
    [MaxLength(50)]
    public string MaterialItemCode { get; set; } = string.Empty;

    public long CategoryId { get; set; }

    public string? Specification { get; set; }

    [MaxLength(20)]
    public string Unit { get; set; } = "PCS";

    public double OnHandQuantity { get; set; } = 0;

    public double? MinStock { get; set; }
    public double? MaxStock { get; set; }
    public double? ReorderLevel { get; set; }
    public double? ReorderQuantity { get; set; }

    [MaxLength(100)]
    public string? Location { get; set; }

    [MaxLength(100)]
    public string? Manufacturer { get; set; }

    [MaxLength(200)]
    public string? Supplier { get; set; }

    [MaxLength(100)]
    public string? PartNumber { get; set; }

    [MaxLength(50)]
    public string? Barcode { get; set; }

    public decimal? UnitCost { get; set; }

    [MaxLength(3)]
    public string? Currency { get; set; } = "USD";

    public string? Notes { get; set; }

    [MaxLength(500)]
    public string? ImageUrl { get; set; }

    public bool IsActive { get; set; } = true;

    /// <summary>Vessel this ship material item belongs to (null = fleet-wide/unassigned)</summary>
    public Guid? VesselId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(MaterialItemCode))]
    [System.Text.Json.Serialization.JsonIgnore]
    public virtual MaterialItem? MaterialItem { get; set; }
}

// ============================================================
// MATERIALS — MATERIAL-EQUIPMENT LINK (M:N)
// ============================================================

public class MaterialItemEquipment
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>FK → DANH MỤC vật tư material_items.Id (loại vật tư dùng cho thiết bị).</summary>
    [Required]
    public Guid MaterialItemId { get; set; }

    [Required]
    public Guid EquipmentAssetId { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

// ============================================================
// MATERIALS — STORE LOCATIONS
// ============================================================

public class StoreLocation
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(50)]
    public string LocationCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public Guid? ParentId { get; set; }

    [MaxLength(300)]
    public string? Address { get; set; }

    [MaxLength(100)]
    public string? ManagerName { get; set; }

    [MaxLength(50)]
    public string? Phone { get; set; }

    [MaxLength(100)]
    public string? Email { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("ParentId")]
    [System.Text.Json.Serialization.JsonIgnore]
    public virtual StoreLocation? Parent { get; set; }


    /// <summary>Tàu sở hữu bản ghi — suy ra từ IMO của node gửi khi nhận đồng bộ.</summary>
    public Guid? VesselId { get; set; }

    public virtual ICollection<StoreLocation> Children { get; set; } = new List<StoreLocation>();
}

// ============================================================
// MATERIALS — MATERIAL REQUESTS
// ============================================================

public class MaterialRequest
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string RequestCode { get; set; } = string.Empty;

    [MaxLength(150)]
    public string? VesselName { get; set; }

    [MaxLength(20)]
    public string Urgency { get; set; } = "Normal";

    public DateTime NeededDate { get; set; } = DateTime.UtcNow;

    public DateTime RequestDate { get; set; } = DateTime.UtcNow;

    [MaxLength(100)]
    public string? RequestedBy { get; set; }

    public string? Notes { get; set; }

    public string? Attachments { get; set; }

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "Draft";

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(50)]
    public string OriginNode { get; set; } = "SHORE";


    /// <summary>Tàu sở hữu bản ghi — suy ra từ IMO của node gửi khi nhận đồng bộ.</summary>
    public Guid? VesselId { get; set; }

    public virtual ICollection<MaterialRequestItem> Items { get; set; } = new List<MaterialRequestItem>();
}

public class MaterialRequestItem
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int RequestId { get; set; }

    public Guid? EquipmentAssetId { get; set; }

    /// <summary>FK → DANH MỤC vật tư material_items.Id (yêu cầu theo loại vật tư). Null = freeform.</summary>
    public Guid? MaterialItemId { get; set; }

    [Required]
    [MaxLength(200)]
    public string ItemName { get; set; } = string.Empty;

    public string? Description { get; set; }

    [MaxLength(20)]
    public string Unit { get; set; } = "PCS";

    public decimal QuantityOnHand { get; set; } = 0;

    [Required]
    public decimal QuantityRequested { get; set; }

    public string? Note { get; set; }

    [ForeignKey("RequestId")]
    [System.Text.Json.Serialization.JsonIgnore]
    public virtual MaterialRequest Request { get; set; } = null!;
}

// ============================================================
// MATERIALS — STOCK RECEIPTS
// ============================================================

public class StockReceipt
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string ReceiptCode { get; set; } = string.Empty;

    [MaxLength(150)]
    public string? VesselName { get; set; }

    [MaxLength(50)]
    public string? SupplierCode { get; set; }

    [MaxLength(200)]
    public string? SupplierName { get; set; }

    public DateTime ReceivedDate { get; set; } = DateTime.UtcNow;

    public DateTime ReceiptDate { get; set; } = DateTime.UtcNow;

    [MaxLength(100)]
    public string? CreatedBy { get; set; }

    public string? Notes { get; set; }

    public string? Attachments { get; set; }

    public int? MaterialRequestId { get; set; }

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "Draft";

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(50)]
    public string OriginNode { get; set; } = "SHORE";


    /// <summary>Tàu sở hữu bản ghi — suy ra từ IMO của node gửi khi nhận đồng bộ.</summary>
    public Guid? VesselId { get; set; }

    public virtual MaterialRequest? MaterialRequest { get; set; }
    public virtual ICollection<StockReceiptItem> Items { get; set; } = new List<StockReceiptItem>();
}

public class StockReceiptItem
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int ReceiptId { get; set; }

    public Guid? StoreLocationId { get; set; }

    public Guid? MaterialItemId { get; set; }

    [MaxLength(50)]
    public string? ItemCode { get; set; }

    [Required]
    [MaxLength(200)]
    public string ItemName { get; set; } = string.Empty;

    public string? Description { get; set; }

    [MaxLength(20)]
    public string Unit { get; set; } = "PCS";

    public decimal QuantityRequested { get; set; } = 0;

    [Required]
    public decimal QuantityReceived { get; set; }

    public decimal? UnitCost { get; set; }

    [MaxLength(3)]
    public string? Currency { get; set; } = "USD";

    public string? Note { get; set; }

    [ForeignKey("ReceiptId")]
    [System.Text.Json.Serialization.JsonIgnore]
    public virtual StockReceipt Receipt { get; set; } = null!;
}

// ============================================================
// MATERIALS — INVENTORY STOCK
// ============================================================

public class InventoryStock
{
    [Key]
    public int Id { get; set; }

    [Required]
    public Guid MaterialItemId { get; set; }

    [Required]
    public Guid StoreLocationId { get; set; }

    public decimal Quantity { get; set; } = 0;

    public decimal UnitCost { get; set; } = 0;

    public DateTime? LastReceiptDate { get; set; }

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(50)]
    public string OriginNode { get; set; } = "SHORE";

    /// <summary>Tàu sở hữu bản ghi — suy ra từ IMO của node gửi khi nhận đồng bộ.</summary>
    public Guid? VesselId { get; set; }
}
