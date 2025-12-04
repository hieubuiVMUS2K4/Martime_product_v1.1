using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MaritimeEdge.Models;

/// <summary>
/// Phiếu nhập thiết bị - KHÔNG quản lý giá trị tài chính
/// </summary>
[Table("equipment_receipts")]
public class EquipmentReceipt
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    /// <summary>
    /// Mã phiếu nhập duy nhất (Format: EQ-YYYYMMDD-XXX)
    /// </summary>
    [Required]
    [MaxLength(50)]
    [Column("receipt_code")]
    public string ReceiptCode { get; set; } = string.Empty;

    /// <summary>
    /// Ngày nhập thiết bị
    /// </summary>
    [Required]
    [Column("receipt_date")]
    public DateTime ReceiptDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Trạng thái phiếu: Draft, Approved, Completed, Cancelled
    /// </summary>
    [Required]
    [MaxLength(50)]
    [Column("status")]
    public string Status { get; set; } = "Draft";

    /// <summary>
    /// Ghi chú chung cho phiếu nhập
    /// </summary>
    [Column("notes")]
    public string? Notes { get; set; }

    /// <summary>
    /// Người tạo phiếu
    /// </summary>
    [MaxLength(100)]
    [Column("created_by")]
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Ngày duyệt phiếu
    /// </summary>
    [Column("approved_date")]
    public DateTime? ApprovedDate { get; set; }

    /// <summary>
    /// Nguồn import: Manual, Excel, API
    /// </summary>
    [MaxLength(50)]
    [Column("import_source")]
    public string? ImportSource { get; set; }

    /// <summary>
    /// Tên file Excel nếu import từ file
    /// </summary>
    [MaxLength(255)]
    [Column("import_file_name")]
    public string? ImportFileName { get; set; }

    /// <summary>
    /// Trạng thái active
    /// </summary>
    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    /// <summary>
    /// Danh sách các thiết bị trong phiếu nhập
    /// </summary>
    public virtual ICollection<EquipmentReceiptItem> ReceiptItems { get; set; } = new List<EquipmentReceiptItem>();
}

/// <summary>
/// Chi tiết thiết bị trong phiếu nhập
/// </summary>
[Table("equipment_receipt_items")]
public class EquipmentReceiptItem
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    /// <summary>
    /// ID phiếu nhập
    /// </summary>
    [Required]
    [Column("receipt_id")]
    public int ReceiptId { get; set; }

    /// <summary>
    /// ID thiết bị (EquipmentItem uses Guid as primary key)
    /// </summary>
    [Required]
    [Column("equipment_item_id")]
    public Guid EquipmentItemId { get; set; }

    /// <summary>
    /// Số thứ tự dòng trong phiếu
    /// </summary>
    [Column("line_number")]
    public int? LineNumber { get; set; }

    // ========================================
    // Snapshot data (chụp lại thông tin tại thời điểm nhập)
    // ========================================
    
    [MaxLength(50)]
    [Column("equipment_code")]
    public string? EquipmentCode { get; set; }

    [MaxLength(200)]
    [Column("equipment_name")]
    public string? EquipmentName { get; set; }

    [MaxLength(200)]
    [Column("category_name")]
    public string? CategoryName { get; set; }

    // ========================================
    // Thông tin nhập kho
    // ========================================

    /// <summary>
    /// Số lượng nhập trong lần này (thêm vào kho)
    /// </summary>
    [Required]
    [Column("quantity")]
    public double Quantity { get; set; }

    [MaxLength(100)]
    [Column("location")]
    public string? Location { get; set; }

    [MaxLength(100)]
    [Column("manufacturer")]
    public string? Manufacturer { get; set; }

    [MaxLength(100)]
    [Column("model")]
    public string? Model { get; set; }

    [MaxLength(100)]
    [Column("serial_number")]
    public string? SerialNumber { get; set; }

    [MaxLength(200)]
    [Column("solas_reference")]
    public string? SolasReference { get; set; }

    [Column("specification")]
    public string? Specification { get; set; }

    [Column("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Ghi chú riêng cho dòng này
    /// </summary>
    [Column("notes")]
    public string? Notes { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    /// <summary>
    /// Phiếu nhập
    /// </summary>
    [ForeignKey("ReceiptId")]
    public virtual EquipmentReceipt Receipt { get; set; } = null!;

    /// <summary>
    /// Thiết bị
    /// </summary>
    [ForeignKey("EquipmentItemId")]
    public virtual EquipmentItem EquipmentItem { get; set; } = null!;
}
