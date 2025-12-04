using System.ComponentModel.DataAnnotations;

namespace MaritimeEdge.DTOs;

// ============================================
// Import Equipment Receipt DTOs
// ============================================

/// <summary>
/// DTO cho việc import phiếu nhập thiết bị từ Excel
/// </summary>
public class ImportEquipmentReceiptDto
{
    [Required]
    public DateTime ReceiptDate { get; set; }
    
    public string? Notes { get; set; }
    
    public string? CreatedBy { get; set; }
    
    public string? ImportSource { get; set; } = "Excel";
    
    public string? ImportFileName { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "At least one equipment item is required")]
    public List<ImportEquipmentReceiptItemDto> Items { get; set; } = new();
}

/// <summary>
/// DTO cho mỗi thiết bị trong phiếu nhập
/// </summary>
public class ImportEquipmentReceiptItemDto
{
    [Required]
    [MaxLength(50)]
    public string EquipmentCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string EquipmentName { get; set; } = string.Empty;

    public string? CategoryName { get; set; }
    
    public long? CategoryId { get; set; } // long because EquipmentCategory.Id is long

    [Required]
    [Range(0.001, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
    public double Quantity { get; set; }

    // Thông tin thiết bị
    public string? Location { get; set; }
    public string? Manufacturer { get; set; }
    public string? Model { get; set; }
    public string? SerialNumber { get; set; }
    public string? SolasReference { get; set; }
    public string? Specification { get; set; }
    public string? Description { get; set; }
    public string? Notes { get; set; }
    public int? LineNumber { get; set; }
    
    // Status cho thiết bị mới
    public string? Status { get; set; } = "OPERATIONAL";
}

/// <summary>
/// Response DTO cho preview trước khi import
/// </summary>
public class EquipmentReceiptPreviewResponseDto
{
    public EquipmentReceiptPreviewSummaryDto Summary { get; set; } = new();
    public List<EquipmentReceiptPreviewItemDto> Items { get; set; } = new();
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}

/// <summary>
/// Thống kê tổng quan cho preview
/// </summary>
public class EquipmentReceiptPreviewSummaryDto
{
    public int TotalItems { get; set; }
    public int NewItems { get; set; }
    public int ExistingItems { get; set; }
    public int ErrorItems { get; set; }
}

/// <summary>
/// Chi tiết từng thiết bị trong preview
/// </summary>
public class EquipmentReceiptPreviewItemDto
{
    public int LineNumber { get; set; }
    public string EquipmentCode { get; set; } = string.Empty;
    public string EquipmentName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public double Quantity { get; set; }
    public string? Location { get; set; }
    public string? Manufacturer { get; set; }
    public string? Model { get; set; }
    public string? SerialNumber { get; set; }
    
    /// <summary>
    /// CREATE: Tạo mới thiết bị
    /// UPDATE: Cập nhật số lượng thiết bị có sẵn
    /// ERROR: Có lỗi, không thể xử lý
    /// </summary>
    public string Action { get; set; } = "CREATE"; // CREATE, UPDATE, ERROR
    
    public Guid? ExistingEquipmentId { get; set; } // Guid because EquipmentItem.Id is Guid
    public double? CurrentQuantity { get; set; }
    public double? NewQuantity { get; set; }
    
    public string? ErrorMessage { get; set; }
    public string? WarningMessage { get; set; }
}

// ============================================
// Equipment Receipt Response DTOs
// ============================================

/// <summary>
/// Response DTO khi tạo/lấy phiếu nhập thành công
/// </summary>
public class EquipmentReceiptResponseDto
{
    public int Id { get; set; }
    public string ReceiptCode { get; set; } = string.Empty;
    public DateTime ReceiptDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public string? ImportSource { get; set; }
    public string? ImportFileName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    public int ItemCount { get; set; }
    public List<EquipmentReceiptItemResponseDto> Items { get; set; } = new();
}

/// <summary>
/// Response DTO cho chi tiết thiết bị trong phiếu
/// </summary>
public class EquipmentReceiptItemResponseDto
{
    public int Id { get; set; }
    public int LineNumber { get; set; }
    public Guid EquipmentItemId { get; set; } // Guid because EquipmentItem.Id is Guid
    public string EquipmentCode { get; set; } = string.Empty;
    public string EquipmentName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public double Quantity { get; set; }
    public string? Location { get; set; }
    public string? Manufacturer { get; set; }
    public string? Model { get; set; }
    public string? SerialNumber { get; set; }
    public string? SolasReference { get; set; }
    public string? Specification { get; set; }
    public string? Description { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// DTO cho danh sách phiếu nhập (list view)
/// </summary>
public class EquipmentReceiptListDto
{
    public int Id { get; set; }
    public string ReceiptCode { get; set; } = string.Empty;
    public DateTime ReceiptDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public int ItemCount { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Query parameters cho lọc phiếu nhập thiết bị
/// </summary>
public class EquipmentReceiptQueryDto
{
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? Status { get; set; }
    public string? SearchTerm { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

/// <summary>
/// Paged response cho danh sách phiếu nhập
/// </summary>
public class EquipmentReceiptPagedResponseDto
{
    public List<EquipmentReceiptListDto> Data { get; set; } = new();
    public int TotalRecords { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
