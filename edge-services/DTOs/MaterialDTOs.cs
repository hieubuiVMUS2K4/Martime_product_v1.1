using System.ComponentModel.DataAnnotations;

namespace MaritimeEdge.DTOs;

// ============================================================
// MATERIAL CATEGORY DTOs
// ============================================================

/// <summary>
/// DTO for creating a new material category
/// </summary>
public class CreateMaterialCategoryDto
{
    [Required(ErrorMessage = "CategoryCode is required")]
    [StringLength(50, MinimumLength = 1, ErrorMessage = "CategoryCode must be between 1 and 50 characters")]
    public string CategoryCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Name is required")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Name must be between 1 and 200 characters")]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public string? Description { get; set; }

    public long? ParentCategoryId { get; set; }

    public bool IsActive { get; set; } = true;
}

/// <summary>
/// DTO for updating an existing material category
/// </summary>
public class UpdateMaterialCategoryDto
{
    [Required(ErrorMessage = "CategoryCode is required")]
    [StringLength(50, MinimumLength = 1, ErrorMessage = "CategoryCode must be between 1 and 50 characters")]
    public string CategoryCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Name is required")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Name must be between 1 and 200 characters")]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public string? Description { get; set; }

    public long? ParentCategoryId { get; set; }

    public bool IsActive { get; set; } = true;
}

/// <summary>
/// DTO for material category response with additional info
/// </summary>
public class MaterialCategoryResponseDto
{
    public long Id { get; set; }
    public string CategoryCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public long? ParentCategoryId { get; set; }
    public string? ParentCategoryName { get; set; }
    public bool IsActive { get; set; }
    public bool IsSynced { get; set; }
    public DateTime CreatedAt { get; set; }
    public int ItemCount { get; set; }
    public int SubCategoryCount { get; set; }
}

// ============================================================
// MATERIAL ITEM DTOs
// ============================================================

/// <summary>
/// DTO for creating a new material item
/// </summary>
public class CreateMaterialItemDto
{
    [Required(ErrorMessage = "ItemCode is required")]
    [StringLength(50, MinimumLength = 1, ErrorMessage = "ItemCode must be between 1 and 50 characters")]
    public string ItemCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Name is required")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Name must be between 1 and 200 characters")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "CategoryId is required")]
    [Range(1, long.MaxValue, ErrorMessage = "CategoryId must be a positive number")]
    public long CategoryId { get; set; }

    [StringLength(500, ErrorMessage = "Specification cannot exceed 500 characters")]
    public string? Specification { get; set; }

    [Required(ErrorMessage = "Unit is required")]
    [StringLength(20, ErrorMessage = "Unit cannot exceed 20 characters")]
    public string Unit { get; set; } = "PCS";

    [Range(0, double.MaxValue, ErrorMessage = "OnHandQuantity must be non-negative")]
    public double OnHandQuantity { get; set; } = 0;

    [Range(0, double.MaxValue, ErrorMessage = "MinStock must be non-negative")]
    public double? MinStock { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "MaxStock must be non-negative")]
    public double? MaxStock { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "ReorderLevel must be non-negative")]
    public double? ReorderLevel { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "ReorderQuantity must be non-negative")]
    public double? ReorderQuantity { get; set; }

    [StringLength(100, ErrorMessage = "Location cannot exceed 100 characters")]
    public string? Location { get; set; }

    [StringLength(100, ErrorMessage = "Manufacturer cannot exceed 100 characters")]
    public string? Manufacturer { get; set; }

    [StringLength(200, ErrorMessage = "Supplier cannot exceed 200 characters")]
    public string? Supplier { get; set; }

    [StringLength(100, ErrorMessage = "PartNumber cannot exceed 100 characters")]
    public string? PartNumber { get; set; }

    [StringLength(50, ErrorMessage = "Barcode cannot exceed 50 characters")]
    public string? Barcode { get; set; }

    public bool BatchTracked { get; set; } = false;
    public bool SerialTracked { get; set; } = false;
    public bool ExpiryRequired { get; set; } = false;

    [Range(0, double.MaxValue, ErrorMessage = "UnitCost must be non-negative")]
    public decimal? UnitCost { get; set; }

    [StringLength(3, ErrorMessage = "Currency must be 3 characters (e.g., USD, EUR, VND)")]
    public string? Currency { get; set; } = "USD";

    [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
    public string? Notes { get; set; }

    public bool IsActive { get; set; } = true;
}

/// <summary>
/// DTO for updating an existing material item
/// </summary>
public class UpdateMaterialItemDto
{
    [Required(ErrorMessage = "ItemCode is required")]
    [StringLength(50, MinimumLength = 1, ErrorMessage = "ItemCode must be between 1 and 50 characters")]
    public string ItemCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Name is required")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Name must be between 1 and 200 characters")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "CategoryId is required")]
    [Range(1, long.MaxValue, ErrorMessage = "CategoryId must be a positive number")]
    public long CategoryId { get; set; }

    [StringLength(500, ErrorMessage = "Specification cannot exceed 500 characters")]
    public string? Specification { get; set; }

    [Required(ErrorMessage = "Unit is required")]
    [StringLength(20, ErrorMessage = "Unit cannot exceed 20 characters")]
    public string Unit { get; set; } = "PCS";

    [Range(0, double.MaxValue, ErrorMessage = "OnHandQuantity must be non-negative")]
    public double OnHandQuantity { get; set; } = 0;

    [Range(0, double.MaxValue, ErrorMessage = "MinStock must be non-negative")]
    public double? MinStock { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "MaxStock must be non-negative")]
    public double? MaxStock { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "ReorderLevel must be non-negative")]
    public double? ReorderLevel { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "ReorderQuantity must be non-negative")]
    public double? ReorderQuantity { get; set; }

    [StringLength(100, ErrorMessage = "Location cannot exceed 100 characters")]
    public string? Location { get; set; }

    [StringLength(100, ErrorMessage = "Manufacturer cannot exceed 100 characters")]
    public string? Manufacturer { get; set; }

    [StringLength(200, ErrorMessage = "Supplier cannot exceed 200 characters")]
    public string? Supplier { get; set; }

    [StringLength(100, ErrorMessage = "PartNumber cannot exceed 100 characters")]
    public string? PartNumber { get; set; }

    [StringLength(50, ErrorMessage = "Barcode cannot exceed 50 characters")]
    public string? Barcode { get; set; }

    public bool BatchTracked { get; set; } = false;
    public bool SerialTracked { get; set; } = false;
    public bool ExpiryRequired { get; set; } = false;

    [Range(0, double.MaxValue, ErrorMessage = "UnitCost must be non-negative")]
    public decimal? UnitCost { get; set; }

    [StringLength(3, ErrorMessage = "Currency must be 3 characters (e.g., USD, EUR, VND)")]
    public string? Currency { get; set; } = "USD";

    [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
    public string? Notes { get; set; }

    public bool IsActive { get; set; } = true;
}

/// <summary>
/// DTO for material item response with additional info
/// </summary>
public class MaterialItemResponseDto
{
    public long Id { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public long CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? CategoryCode { get; set; }
    public string? Specification { get; set; }
    public string Unit { get; set; } = "PCS";
    public double OnHandQuantity { get; set; }
    public double? MinStock { get; set; }
    public double? MaxStock { get; set; }
    public double? ReorderLevel { get; set; }
    public double? ReorderQuantity { get; set; }
    public string? Location { get; set; }
    public string? Manufacturer { get; set; }
    public string? Supplier { get; set; }
    public string? PartNumber { get; set; }
    public string? Barcode { get; set; }
    public bool BatchTracked { get; set; }
    public bool SerialTracked { get; set; }
    public bool ExpiryRequired { get; set; }
    public decimal? UnitCost { get; set; }
    public string? Currency { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
    public bool IsSynced { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Computed fields
    public bool IsLowStock { get; set; }
    public decimal? TotalValue { get; set; }
    public string StockStatus { get; set; } = "OK";
}

/// <summary>
/// DTO for bulk operations response
/// </summary>
public class BulkOperationResultDto
{
    public int SuccessCount { get; set; }
    public int FailedCount { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<long> SuccessfulIds { get; set; } = new();
}

/// <summary>
/// DTO for stock adjustment
/// </summary>
public class StockAdjustmentDto
{
    [Required(ErrorMessage = "ItemId is required")]
    [Range(1, long.MaxValue, ErrorMessage = "ItemId must be a positive number")]
    public long ItemId { get; set; }

    [Required(ErrorMessage = "Quantity is required")]
    public double Quantity { get; set; }

    [Required(ErrorMessage = "AdjustmentType is required")]
    [RegularExpression("^(Add|Subtract|Set)$", ErrorMessage = "AdjustmentType must be 'Add', 'Subtract', or 'Set'")]
    public string AdjustmentType { get; set; } = "Add"; // Add, Subtract, Set

    [StringLength(500, ErrorMessage = "Reason cannot exceed 500 characters")]
    public string? Reason { get; set; }
}
