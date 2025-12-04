using System;
using System.ComponentModel.DataAnnotations;

namespace MaritimeEdge.DTOs
{
    // ==================== EQUIPMENT CATEGORY DTOs ====================
    
    public class CreateEquipmentCategoryDto
    {
        [Required]
        [StringLength(50)]
        public string CategoryCode { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateEquipmentCategoryDto
    {
        [StringLength(50)]
        public string? CategoryCode { get; set; }

        [StringLength(200)]
        public string? Name { get; set; }

        public string? Description { get; set; }

        public bool? IsActive { get; set; }
    }

    public class EquipmentCategoryResponseDto
    {
        public long Id { get; set; }
        public string CategoryCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public int EquipmentCount { get; set; }
    }

    // ==================== EQUIPMENT ITEM DTOs ====================
    
    public class CreateEquipmentItemDto
    {
        [Required]
        public long CategoryId { get; set; }

        [Required]
        [StringLength(50)]
        public string EquipmentCode { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [StringLength(100)]
        public string? Location { get; set; }

        public string? Specification { get; set; }

        [StringLength(100)]
        public string? Manufacturer { get; set; }

        [StringLength(100)]
        public string? Model { get; set; }

        [StringLength(100)]
        public string? SerialNumber { get; set; }

        [StringLength(200)]
        public string? SolasReference { get; set; }

        [Required]
        [Range(0.1, double.MaxValue)]
        public double Quantity { get; set; } = 1.0;

        [Required]
        [StringLength(30)]
        public string Status { get; set; } = "OPERATIONAL";

        public bool IsActive { get; set; } = true;

        [StringLength(50)]
        public string OriginNode { get; set; } = "SHIP_01";
    }

    public class UpdateEquipmentItemDto
    {
        public long? CategoryId { get; set; }

        [StringLength(50)]
        public string? EquipmentCode { get; set; }

        [StringLength(200)]
        public string? Name { get; set; }

        public string? Description { get; set; }

        [StringLength(100)]
        public string? Location { get; set; }

        public string? Specification { get; set; }

        [StringLength(100)]
        public string? Manufacturer { get; set; }

        [StringLength(100)]
        public string? Model { get; set; }

        [StringLength(100)]
        public string? SerialNumber { get; set; }

        [StringLength(200)]
        public string? SolasReference { get; set; }

        [Range(0.1, double.MaxValue)]
        public double? Quantity { get; set; }

        [StringLength(30)]
        public string? Status { get; set; }

        public bool? IsActive { get; set; }

        [StringLength(50)]
        public string? OriginNode { get; set; }
    }

    public class EquipmentItemResponseDto
    {
        public Guid Id { get; set; }
        public long CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string CategoryCode { get; set; } = string.Empty;
        public string EquipmentCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Location { get; set; }
        public string? Specification { get; set; }
        public string? Manufacturer { get; set; }
        public string? Model { get; set; }
        public string? SerialNumber { get; set; }
        public string? SolasReference { get; set; }
        public double Quantity { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsSynced { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string OriginNode { get; set; } = string.Empty;
    }

    // Query DTOs for filtering and pagination
    public class EquipmentItemQueryDto
    {
        public long? CategoryId { get; set; }
        public string? Status { get; set; }
        public bool? IsActive { get; set; }
        public string? SearchTerm { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
