using System.ComponentModel.DataAnnotations;

namespace MaritimeEdge.DTOs;

// ============================================
// Import Receipt DTOs
// ============================================

/// <summary>
/// DTO cho việc import phiếu nhập từ Excel
/// </summary>
public class ImportReceiptDto
{
    [Required]
    public DateTime ReceiptDate { get; set; }
    
    public string? Notes { get; set; }
    
    public string? CreatedBy { get; set; }
    
    public string? ImportSource { get; set; } = "Excel";
    
    public string? ImportFileName { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "At least one item is required")]
    public List<ImportReceiptItemDto> Items { get; set; } = new();
}

/// <summary>
/// DTO cho mỗi vật tư trong phiếu nhập
/// </summary>
public class ImportReceiptItemDto
{
    [Required]
    [MaxLength(50)]
    public string ItemCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string ItemName { get; set; } = string.Empty;

    public string? CategoryName { get; set; }
    
    public long? CategoryId { get; set; } // long because MaterialCategory.Id is long

    [Required]
    [Range(0.001, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
    public decimal Quantity { get; set; }

    [Required]
    [MaxLength(50)]
    public string Unit { get; set; } = string.Empty;

    public decimal? UnitCost { get; set; }
    
    public string? Currency { get; set; } = "USD";

    // Optional fields
    public string? Location { get; set; }
    public string? PartNumber { get; set; }
    public string? Barcode { get; set; }
    public string? Manufacturer { get; set; }
    public string? Specification { get; set; }
    public string? BatchNumber { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? Notes { get; set; }
    public int? LineNumber { get; set; }

    // Stock thresholds
    public decimal? MinStock { get; set; }
    public decimal? MaxStock { get; set; }
    
    // Additional fields
    public string? Supplier { get; set; }
    public decimal? ReorderLevel { get; set; }
    public decimal? ReorderQuantity { get; set; }
}

/// <summary>
/// Response DTO cho preview trước khi import
/// </summary>
public class ReceiptPreviewResponseDto
{
    public ReceiptPreviewSummaryDto Summary { get; set; } = new();
    public List<ReceiptPreviewItemDto> Items { get; set; } = new();
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}

/// <summary>
/// Thống kê tổng quan cho preview
/// </summary>
public class ReceiptPreviewSummaryDto
{
    public int TotalItems { get; set; }
    public int NewItems { get; set; }
    public int ExistingItems { get; set; }
    public int ErrorItems { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "USD";
}

/// <summary>
/// Chi tiết từng item trong preview
/// </summary>
public class ReceiptPreviewItemDto
{
    public int LineNumber { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public decimal? UnitCost { get; set; }
    public decimal? TotalCost { get; set; }
    
    /// <summary>
    /// CREATE: Tạo mới vật tư
    /// UPDATE: Cập nhật số lượng vật tư có sẵn
    /// ERROR: Có lỗi, không thể xử lý
    /// </summary>
    public string Action { get; set; } = "CREATE"; // CREATE, UPDATE, ERROR
    
    public Guid? ExistingMaterialId { get; set; } // Guid because MaterialItem.Id is Guid
    public decimal? CurrentStock { get; set; }
    public decimal? NewStock { get; set; }
    
    public string? ErrorMessage { get; set; }
    public string? WarningMessage { get; set; }
}

// ============================================
// Material Receipt Response DTOs
// ============================================

/// <summary>
/// Response DTO khi tạo/lấy phiếu nhập thành công
/// </summary>
public class MaterialReceiptResponseDto
{
    public int Id { get; set; }
    public string ReceiptCode { get; set; } = string.Empty;
    public DateTime ReceiptDate { get; set; }
    public decimal? TotalAmount { get; set; }
    public string Currency { get; set; } = "USD";
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public string? ImportSource { get; set; }
    public string? ImportFileName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    public int ItemCount { get; set; }
    public List<MaterialReceiptItemResponseDto> Items { get; set; } = new();
}

/// <summary>
/// Response DTO cho chi tiết vật tư trong phiếu
/// </summary>
public class MaterialReceiptItemResponseDto
{
    public int Id { get; set; }
    public int LineNumber { get; set; }
    public Guid? MaterialItemId { get; set; } // Nullable: null nếu vật tư đã bị xóa
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public decimal? UnitCost { get; set; }
    public decimal? TotalCost { get; set; }
    public string Currency { get; set; } = "USD";
    public string? BatchNumber { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// DTO cho danh sách phiếu nhập (list view)
/// </summary>
public class MaterialReceiptListDto
{
    public int Id { get; set; }
    public string ReceiptCode { get; set; } = string.Empty;
    public DateTime ReceiptDate { get; set; }
    public decimal? TotalAmount { get; set; }
    public string Currency { get; set; } = "USD";
    public string Status { get; set; } = string.Empty;
    public int ItemCount { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Query parameters cho lọc phiếu nhập
/// </summary>
public class ReceiptQueryDto
{
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? Status { get; set; }
    public string? SearchTerm { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// Response cho danh sách phiếu nhập có phân trang
/// </summary>
public class MaterialReceiptPagedResponseDto
{
    public List<MaterialReceiptListDto> Data { get; set; } = new();
    public int TotalRecords { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
