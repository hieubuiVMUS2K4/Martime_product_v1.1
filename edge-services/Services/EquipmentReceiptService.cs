using MaritimeEdge.Data;
using MaritimeEdge.DTOs;
using MaritimeEdge.Models;
using Microsoft.EntityFrameworkCore;

namespace MaritimeEdge.Services;

/// <summary>
/// Service xử lý logic import phiếu nhập thiết bị
/// KHÔNG quản lý giá trị tài chính, chỉ tracking số lượng thiết bị
/// </summary>
public class EquipmentReceiptService
{
    private readonly EdgeDbContext _context;
    private readonly ILogger<EquipmentReceiptService> _logger;

    public EquipmentReceiptService(
        EdgeDbContext context,
        ILogger<EquipmentReceiptService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Preview dữ liệu trước khi import - Kiểm tra items nào tạo mới, items nào update
    /// </summary>
    public async Task<EquipmentReceiptPreviewResponseDto> PreviewImportAsync(ImportEquipmentReceiptDto dto)
    {
        var response = new EquipmentReceiptPreviewResponseDto();
        var errors = new List<string>();
        var warnings = new List<string>();

        // Validate basic data
        if (!dto.Items.Any())
        {
            errors.Add("No equipment items found in the import data");
            response.Errors = errors;
            return response;
        }

        // Lấy tất cả equipment items hiện có để check duplicate
        var existingItems = await _context.EquipmentItems
            .ToListAsync();

        var existingItemCodes = existingItems.ToDictionary(e => e.EquipmentCode, e => e);
        
        // Lấy tất cả categories để lookup tên
        var categories = await _context.EquipmentCategories
            .Where(c => c.IsActive)
            .ToListAsync();
        var categoryDict = categories.ToDictionary(c => c.Id, c => c.Name);

        int newItemCount = 0;
        int existingItemCount = 0;
        int errorItemCount = 0;

        foreach (var item in dto.Items)
        {
            var previewItem = new EquipmentReceiptPreviewItemDto
            {
                LineNumber = item.LineNumber ?? (dto.Items.IndexOf(item) + 1),
                EquipmentCode = item.EquipmentCode,
                EquipmentName = item.EquipmentName,
                CategoryName = item.CategoryName ?? "General",
                Quantity = item.Quantity,
                Location = item.Location,
                Manufacturer = item.Manufacturer,
                Model = item.Model,
                SerialNumber = item.SerialNumber
            };

            // Kiểm tra equipment đã tồn tại chưa
            if (existingItemCodes.TryGetValue(item.EquipmentCode, out var existingItem))
            {
                // Equipment ĐÃ TỒN TẠI - Sẽ cập nhật số lượng
                previewItem.Action = "UPDATE";
                previewItem.ExistingEquipmentId = existingItem.Id;
                previewItem.CurrentQuantity = existingItem.Quantity;
                previewItem.NewQuantity = existingItem.Quantity + item.Quantity;
                
                // Lookup category name từ dictionary
                if (categoryDict.TryGetValue(existingItem.CategoryId, out var catName))
                {
                    previewItem.CategoryName = catName;
                }

                existingItemCount++;

                // Warning nếu location khác
                if (!string.IsNullOrEmpty(existingItem.Location) && 
                    !string.IsNullOrEmpty(item.Location) && 
                    existingItem.Location != item.Location)
                {
                    previewItem.WarningMessage = $"Location mismatch: Existing '{existingItem.Location}' vs Import '{item.Location}'";
                    warnings.Add($"Line {previewItem.LineNumber}: {previewItem.WarningMessage}");
                }
            }
            else
            {
                // Equipment CHƯA TỒN TẠI - Sẽ tạo mới
                previewItem.Action = "CREATE";
                previewItem.NewQuantity = item.Quantity;
                newItemCount++;

                // Kiểm tra category có tồn tại không
                if (!string.IsNullOrEmpty(item.CategoryName))
                {
                    var category = await _context.EquipmentCategories
                        .FirstOrDefaultAsync(c => c.Name == item.CategoryName && c.IsActive);
                    
                    if (category == null)
                    {
                        previewItem.WarningMessage = $"Category '{item.CategoryName}' not found, will use default";
                        warnings.Add($"Line {previewItem.LineNumber}: {previewItem.WarningMessage}");
                    }
                }
            }

            // Validate data
            if (item.Quantity <= 0)
            {
                previewItem.Action = "ERROR";
                previewItem.ErrorMessage = "Quantity must be greater than 0";
                errors.Add($"Line {previewItem.LineNumber}: {previewItem.ErrorMessage}");
                errorItemCount++;
            }

            if (string.IsNullOrWhiteSpace(item.EquipmentCode))
            {
                previewItem.Action = "ERROR";
                previewItem.ErrorMessage = "Equipment code is required";
                errors.Add($"Line {previewItem.LineNumber}: {previewItem.ErrorMessage}");
                errorItemCount++;
            }

            response.Items.Add(previewItem);
        }

        // Tổng hợp summary
        response.Summary = new EquipmentReceiptPreviewSummaryDto
        {
            TotalItems = dto.Items.Count,
            NewItems = newItemCount,
            ExistingItems = existingItemCount,
            ErrorItems = errorItemCount
        };

        response.Errors = errors;
        response.Warnings = warnings;

        return response;
    }

    /// <summary>
    /// Import phiếu nhập thiết bị - Tạo/cập nhật EquipmentItems và tạo Receipt records
    /// </summary>
    public async Task<EquipmentReceiptResponseDto> ImportReceiptAsync(ImportEquipmentReceiptDto dto)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        
        try
        {
            // 1. Tạo phiếu nhập (Draft status)
            var receipt = new EquipmentReceipt
            {
                ReceiptCode = await GenerateReceiptCodeAsync(),
                ReceiptDate = dto.ReceiptDate.Kind == DateTimeKind.Unspecified 
                    ? DateTime.SpecifyKind(dto.ReceiptDate, DateTimeKind.Utc) 
                    : dto.ReceiptDate.ToUniversalTime(),
                Notes = dto.Notes,
                Status = "Draft",
                ImportSource = dto.ImportSource ?? "Excel",
                ImportFileName = dto.ImportFileName,
                CreatedBy = dto.CreatedBy,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _context.EquipmentReceipts.AddAsync(receipt);
            await _context.SaveChangesAsync();

            var receiptItems = new List<EquipmentReceiptItem>();
            var processedItems = new List<EquipmentReceiptItemResponseDto>();
            
            // Load categories for lookup
            var categories = await _context.EquipmentCategories
                .Where(c => c.IsActive)
                .ToListAsync();

            // 2. Xử lý từng item
            foreach (var itemDto in dto.Items)
            {
                try
                {
                    // 2a. Tìm hoặc tạo EquipmentItem
                    var equipmentItem = await _context.EquipmentItems
                        .FirstOrDefaultAsync(e => e.EquipmentCode == itemDto.EquipmentCode);

                    bool isNewItem = false;

                    if (equipmentItem == null)
                    {
                        // TẠO MỚI EquipmentItem
                        isNewItem = true;

                        // Tìm category
                        long? categoryId = itemDto.CategoryId;
                        if (!categoryId.HasValue && !string.IsNullOrEmpty(itemDto.CategoryName))
                        {
                            var category = await _context.EquipmentCategories
                                .FirstOrDefaultAsync(c => c.Name == itemDto.CategoryName && c.IsActive);
                            categoryId = category?.Id;
                        }

                        equipmentItem = new EquipmentItem
                        {
                            EquipmentCode = itemDto.EquipmentCode,
                            Name = itemDto.EquipmentName,
                            CategoryId = categoryId ?? 1, // Default category if not found
                            Quantity = itemDto.Quantity,
                            Location = itemDto.Location,
                            Manufacturer = itemDto.Manufacturer,
                            Model = itemDto.Model,
                            SerialNumber = itemDto.SerialNumber,
                            SolasReference = itemDto.SolasReference,
                            Specification = itemDto.Specification,
                            Description = itemDto.Description,
                            Status = itemDto.Status ?? "OPERATIONAL",
                            IsActive = true,
                            IsSynced = false,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow,
                            OriginNode = "SHIP_01"
                        };

                        await _context.EquipmentItems.AddAsync(equipmentItem);
                        await _context.SaveChangesAsync();

                        _logger.LogInformation($"Created new EquipmentItem: {equipmentItem.EquipmentCode} - {equipmentItem.Name}");
                    }
                    else
                    {
                        // CẬP NHẬT số lượng EquipmentItem có sẵn
                        equipmentItem.Quantity += itemDto.Quantity;
                        
                        // Cập nhật location nếu có
                        if (!string.IsNullOrEmpty(itemDto.Location))
                        {
                            equipmentItem.Location = itemDto.Location;
                        }

                        // Cập nhật manufacturer/model nếu có
                        if (!string.IsNullOrEmpty(itemDto.Manufacturer))
                        {
                            equipmentItem.Manufacturer = itemDto.Manufacturer;
                        }

                        if (!string.IsNullOrEmpty(itemDto.Model))
                        {
                            equipmentItem.Model = itemDto.Model;
                        }

                        equipmentItem.UpdatedAt = DateTime.UtcNow;

                        _logger.LogInformation($"Updated EquipmentItem: {equipmentItem.EquipmentCode} - Added quantity: {itemDto.Quantity}");
                    }

                    // 2b. Tạo EquipmentReceiptItem (chi tiết phiếu nhập)
                    var receiptItem = new EquipmentReceiptItem
                    {
                        ReceiptId = receipt.Id,
                        EquipmentItemId = equipmentItem.Id,
                        LineNumber = itemDto.LineNumber,
                        
                        // Snapshot data
                        EquipmentCode = equipmentItem.EquipmentCode,
                        EquipmentName = equipmentItem.Name,
                        CategoryName = itemDto.CategoryName,
                        
                        // Thông tin nhập kho
                        Quantity = itemDto.Quantity,
                        Location = itemDto.Location,
                        Manufacturer = itemDto.Manufacturer,
                        Model = itemDto.Model,
                        SerialNumber = itemDto.SerialNumber,
                        SolasReference = itemDto.SolasReference,
                        Specification = itemDto.Specification,
                        Description = itemDto.Description,
                        Notes = itemDto.Notes,
                        
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    await _context.EquipmentReceiptItems.AddAsync(receiptItem);

                    // Lookup category name for response
                    string categoryName = itemDto.CategoryName ?? "Unknown";
                    if (categories.Any(c => c.Id == equipmentItem.CategoryId))
                    {
                        categoryName = categories.First(c => c.Id == equipmentItem.CategoryId).Name;
                    }

                    processedItems.Add(new EquipmentReceiptItemResponseDto
                    {
                        Id = receiptItem.Id,
                        LineNumber = receiptItem.LineNumber ?? 0,
                        EquipmentItemId = equipmentItem.Id,
                        EquipmentCode = equipmentItem.EquipmentCode,
                        EquipmentName = equipmentItem.Name,
                        CategoryName = categoryName,
                        Quantity = receiptItem.Quantity,
                        Location = receiptItem.Location,
                        Manufacturer = receiptItem.Manufacturer,
                        Model = receiptItem.Model,
                        SerialNumber = receiptItem.SerialNumber,
                        SolasReference = receiptItem.SolasReference,
                        Specification = receiptItem.Specification,
                        Description = receiptItem.Description,
                        Notes = receiptItem.Notes
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error processing equipment: {itemDto.EquipmentCode}");
                    throw;
                }
            }

            // 3. Cập nhật status phiếu
            receipt.Status = "Completed";
            receipt.ApprovedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation($"Successfully imported equipment receipt {receipt.ReceiptCode} with {processedItems.Count} items");

            // 4. Return response
            return new EquipmentReceiptResponseDto
            {
                Id = receipt.Id,
                ReceiptCode = receipt.ReceiptCode,
                ReceiptDate = receipt.ReceiptDate,
                Status = receipt.Status,
                Notes = receipt.Notes,
                CreatedBy = receipt.CreatedBy,
                ApprovedDate = receipt.ApprovedDate,
                ImportSource = receipt.ImportSource,
                ImportFileName = receipt.ImportFileName,
                CreatedAt = receipt.CreatedAt,
                UpdatedAt = receipt.UpdatedAt,
                ItemCount = processedItems.Count,
                Items = processedItems
            };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Failed to import equipment receipt");
            throw new Exception($"Equipment import failed: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Lấy danh sách phiếu nhập có phân trang
    /// </summary>
    public async Task<EquipmentReceiptPagedResponseDto> GetReceiptsAsync(EquipmentReceiptQueryDto query)
    {
        var receiptsQuery = _context.EquipmentReceipts
            .Include(r => r.ReceiptItems)
            .AsQueryable();

        // Apply filters
        if (query.FromDate.HasValue)
        {
            receiptsQuery = receiptsQuery.Where(r => r.ReceiptDate >= query.FromDate.Value);
        }

        if (query.ToDate.HasValue)
        {
            receiptsQuery = receiptsQuery.Where(r => r.ReceiptDate <= query.ToDate.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            receiptsQuery = receiptsQuery.Where(r => r.Status == query.Status);
        }

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            receiptsQuery = receiptsQuery.Where(r => 
                r.ReceiptCode.Contains(query.SearchTerm));
        }

        var totalRecords = await receiptsQuery.CountAsync();

        var receipts = await receiptsQuery
            .OrderByDescending(r => r.ReceiptDate)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        var receiptDtos = receipts.Select(r => new EquipmentReceiptListDto
        {
            Id = r.Id,
            ReceiptCode = r.ReceiptCode,
            ReceiptDate = r.ReceiptDate,
            Status = r.Status,
            ItemCount = r.ReceiptItems.Count,
            CreatedBy = r.CreatedBy,
            CreatedAt = r.CreatedAt
        }).ToList();

        return new EquipmentReceiptPagedResponseDto
        {
            Data = receiptDtos,
            TotalRecords = totalRecords,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalPages = (int)Math.Ceiling((double)totalRecords / query.PageSize)
        };
    }

    /// <summary>
    /// Lấy chi tiết một phiếu nhập
    /// </summary>
    public async Task<EquipmentReceiptResponseDto?> GetReceiptByIdAsync(int id)
    {
        var receipt = await _context.EquipmentReceipts
            .Include(r => r.ReceiptItems)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (receipt == null)
            return null;
            
        // Load EquipmentItems manually
        var equipmentItemIds = receipt.ReceiptItems.Select(ri => ri.EquipmentItemId).Distinct().ToList();
        var equipmentItems = await _context.EquipmentItems
            .Where(e => equipmentItemIds.Contains(e.Id))
            .ToListAsync();
        var equipmentItemDict = equipmentItems.ToDictionary(e => e.Id);
        
        // Load Categories
        var categoryIds = equipmentItems.Select(e => e.CategoryId).Distinct().ToList();
        var categories = await _context.EquipmentCategories
            .Where(c => categoryIds.Contains(c.Id))
            .ToListAsync();
        var categoryDict = categories.ToDictionary(c => c.Id, c => c.Name);

        return new EquipmentReceiptResponseDto
        {
            Id = receipt.Id,
            ReceiptCode = receipt.ReceiptCode,
            ReceiptDate = receipt.ReceiptDate,
            Status = receipt.Status,
            Notes = receipt.Notes,
            CreatedBy = receipt.CreatedBy,
            ApprovedDate = receipt.ApprovedDate,
            ImportSource = receipt.ImportSource,
            ImportFileName = receipt.ImportFileName,
            CreatedAt = receipt.CreatedAt,
            UpdatedAt = receipt.UpdatedAt,
            ItemCount = receipt.ReceiptItems.Count,
            Items = receipt.ReceiptItems.Select(ri =>
            {
                var eq = equipmentItemDict.TryGetValue(ri.EquipmentItemId, out var e) ? e : null;
                return new EquipmentReceiptItemResponseDto
                {
                    Id = ri.Id,
                    LineNumber = ri.LineNumber ?? 0,
                    EquipmentItemId = ri.EquipmentItemId,
                    EquipmentCode = ri.EquipmentCode ?? eq?.EquipmentCode ?? "",
                    EquipmentName = ri.EquipmentName ?? eq?.Name ?? "",
                    CategoryName = ri.CategoryName ?? (eq != null && categoryDict.TryGetValue(eq.CategoryId, out var catName) ? catName : ""),
                    Quantity = ri.Quantity,
                    Location = ri.Location,
                    Manufacturer = ri.Manufacturer,
                    Model = ri.Model,
                    SerialNumber = ri.SerialNumber,
                    SolasReference = ri.SolasReference,
                    Specification = ri.Specification,
                    Description = ri.Description,
                    Notes = ri.Notes
                };
            }).ToList()
        };
    }

    /// <summary>
    /// Generate mã phiếu nhập tự động (Format: EQ-YYYYMMDD-XXX)
    /// </summary>
    private async Task<string> GenerateReceiptCodeAsync()
    {
        var today = DateTime.Today;
        var datePrefix = $"EQ-{today:yyyyMMdd}";

        // Lấy số sequence cuối cùng trong ngày
        var lastReceipt = await _context.EquipmentReceipts
            .Where(r => r.ReceiptCode.StartsWith(datePrefix))
            .OrderByDescending(r => r.ReceiptCode)
            .FirstOrDefaultAsync();

        int sequence = 1;
        if (lastReceipt != null)
        {
            var lastSequence = lastReceipt.ReceiptCode.Substring(datePrefix.Length + 1);
            if (int.TryParse(lastSequence, out int lastSeq))
            {
                sequence = lastSeq + 1;
            }
        }

        return $"{datePrefix}-{sequence:D3}";
    }
}
