using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MaritimeEdge.Models.Inventory;

/// <summary>
/// Phiếu nhập kho vật tư
/// </summary>
[Table("MaterialReceipts")]
public class MaterialReceipt
{
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Mã phiếu nhập duy nhất (Format: PN-YYYYMMDD-XXX)
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string ReceiptCode { get; set; } = string.Empty;

    /// <summary>
    /// Ngày nhập kho
    /// </summary>
    [Required]
    public DateTime ReceiptDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Tổng giá trị phiếu nhập
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal? TotalAmount { get; set; }

    /// <summary>
    /// Đơn vị tiền tệ
    /// </summary>
    [MaxLength(10)]
    public string Currency { get; set; } = "USD";

    /// <summary>
    /// Trạng thái phiếu: Draft, Approved, Completed, Cancelled
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = "Draft";

    /// <summary>
    /// Ghi chú chung cho phiếu nhập
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Người tạo phiếu
    /// </summary>
    [MaxLength(100)]
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Ngày duyệt phiếu
    /// </summary>
    public DateTime? ApprovedDate { get; set; }

    /// <summary>
    /// Nguồn import: Manual, Excel, API
    /// </summary>
    [MaxLength(50)]
    public string? ImportSource { get; set; }

    /// <summary>
    /// Tên file Excel nếu import từ file
    /// </summary>
    [MaxLength(255)]
    public string? ImportFileName { get; set; }

    /// <summary>
    /// Trạng thái active
    /// </summary>
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    /// <summary>
    /// Danh sách các vật tư trong phiếu nhập
    /// </summary>
    public virtual ICollection<MaterialReceiptItem> ReceiptItems { get; set; } = new List<MaterialReceiptItem>();
}

/// <summary>
/// Chi tiết vật tư trong phiếu nhập kho
/// </summary>
[Table("MaterialReceiptItems")]
public class MaterialReceiptItem
{
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// ID phiếu nhập
    /// </summary>
    [Required]
    public int ReceiptId { get; set; }

    /// <summary>
    /// ID vật tư (MaterialItem uses Guid as primary key)
    /// Nullable: nếu MaterialItem bị xóa, field này = null nhưng vẫn giữ thông tin snapshot
    /// </summary>
    public Guid? MaterialItemId { get; set; }

    /// <summary>
    /// Mã vật tư (snapshot tại thời điểm nhập)
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string ItemCode { get; set; } = string.Empty;

    /// <summary>
    /// Tên vật tư (snapshot tại thời điểm nhập)
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string ItemName { get; set; } = string.Empty;

    /// <summary>
    /// Thông số kỹ thuật (snapshot tại thời điểm nhập)
    /// </summary>
    public string? Specification { get; set; }

    /// <summary>
    /// Đơn vị (snapshot tại thời điểm nhập)
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string Unit { get; set; } = "PCS";

    /// <summary>
    /// Số lượng nhập trong lần này
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(18,3)")]
    public decimal Quantity { get; set; }

    /// <summary>
    /// Đơn giá tại thời điểm nhập (snapshot)
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal? UnitCost { get; set; }

    /// <summary>
    /// Thành tiền = Quantity * UnitCost
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal? TotalCost { get; set; }

    /// <summary>
    /// Đơn vị tiền tệ
    /// </summary>
    [MaxLength(10)]
    public string Currency { get; set; } = "USD";

    /// <summary>
    /// Số thứ tự dòng trong phiếu
    /// </summary>
    public int? LineNumber { get; set; }

    /// <summary>
    /// Ghi chú riêng cho dòng này
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Số lô hàng (Batch/Lot number)
    /// </summary>
    [MaxLength(100)]
    public string? BatchNumber { get; set; }

    /// <summary>
    /// Hạn sử dụng (cho các vật tư có expiry)
    /// </summary>
    public DateTime? ExpiryDate { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    /// <summary>
    /// Phiếu nhập
    /// </summary>
    [ForeignKey("ReceiptId")]
    public virtual MaterialReceipt Receipt { get; set; } = null!;

    /// <summary>
    /// Vật tư trên tàu (phiếu nhập cập nhật tồn kho theo tàu).
    /// </summary>
    [ForeignKey("MaterialItemId")]
    public virtual MaterialItem? MaterialItem { get; set; }
}
