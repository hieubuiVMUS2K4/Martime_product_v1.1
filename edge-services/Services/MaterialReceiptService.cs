using MaritimeEdge.Data;
using MaritimeEdge.DTOs;
using MaritimeEdge.Models;
using Microsoft.EntityFrameworkCore;

namespace MaritimeEdge.Services;

/// <summary>
/// Service xử lý logic import phiếu nhập kho vật tư
/// </summary>
public class MaterialReceiptService
{
    private readonly EdgeDbContext _context;
    private readonly ILogger<MaterialReceiptService> _logger;

    public MaterialReceiptService(
        EdgeDbContext context,
        ILogger<MaterialReceiptService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Preview dữ liệu trước khi import - Kiểm tra items nào tạo mới, items nào update
    /// </summary>
    public async Task<ReceiptPreviewResponseDto> PreviewImportAsync(ImportReceiptDto dto)
    {
        var response = new ReceiptPreviewResponseDto();
        var errors = new List<string>();
        var warnings = new List<string>();

        // Validate basic data
        if (!dto.Items.Any())
        {
            errors.Add("No items found in the import data");
            response.Errors = errors;
            return response;
        }

        // Lấy tất cả material items hiện có để check duplicate
        var existingItems = await _context.MaterialItems
            .ToListAsync();

        var existingItemCodes = existingItems.ToDictionary(m => m.ItemCode, m => m);
        
        // Lấy tất cả categories để lookup tên
        var categories = await _context.MaterialCategories
            .Where(c => c.IsActive)
            .ToListAsync();
        var categoryDict = categories.ToDictionary(c => c.Id, c => c.Name);

        decimal totalAmount = 0;
        int newItemCount = 0;
        int existingItemCount = 0;
        int errorItemCount = 0;

        foreach (var item in dto.Items)
        {
            var previewItem = new ReceiptPreviewItemDto
            {
                LineNumber = item.LineNumber ?? (dto.Items.IndexOf(item) + 1),
                ItemCode = item.ItemCode,
                ItemName = item.ItemName,
                CategoryName = item.CategoryName ?? "General",
                Quantity = item.Quantity,
                Unit = item.Unit,
                UnitCost = item.UnitCost,
                TotalCost = item.UnitCost * item.Quantity
            };

            // Kiểm tra item đã tồn tại chưa
            if (existingItemCodes.TryGetValue(item.ItemCode, out var existingItem))
            {
                // Item ĐÃ TỒN TẠI - Sẽ cập nhật số lượng
                previewItem.Action = "UPDATE";
                previewItem.ExistingMaterialId = existingItem.Id;
                previewItem.CurrentStock = (decimal)existingItem.OnHandQuantity;
                previewItem.NewStock = (decimal)(existingItem.OnHandQuantity + (double)item.Quantity);
                // Lookup category name từ dictionary
                if (categoryDict.TryGetValue(existingItem.CategoryId, out var catName))
                {
                    previewItem.CategoryName = catName;
                }

                existingItemCount++;

                // Warning nếu đơn vị không khớp
                if (existingItem.Unit != item.Unit)
                {
                    previewItem.WarningMessage = $"Unit mismatch: Existing '{existingItem.Unit}' vs Import '{item.Unit}'";
                    warnings.Add($"Line {previewItem.LineNumber}: {previewItem.WarningMessage}");
                }
            }
            else
            {
                // Item CHƯA TỒN TẠI - Sẽ tạo mới
                previewItem.Action = "CREATE";
                previewItem.NewStock = item.Quantity;
                newItemCount++;

                // Kiểm tra category có tồn tại không
                if (!string.IsNullOrEmpty(item.CategoryName))
                {
                    var category = await _context.MaterialCategories
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

            if (string.IsNullOrWhiteSpace(item.ItemCode))
            {
                previewItem.Action = "ERROR";
                previewItem.ErrorMessage = "Item code is required";
                errors.Add($"Line {previewItem.LineNumber}: {previewItem.ErrorMessage}");
                errorItemCount++;
            }

            if (item.UnitCost.HasValue)
            {
                totalAmount += previewItem.TotalCost ?? 0;
            }

            response.Items.Add(previewItem);
        }

        // Tổng hợp summary
        response.Summary = new ReceiptPreviewSummaryDto
        {
            TotalItems = dto.Items.Count,
            NewItems = newItemCount,
            ExistingItems = existingItemCount,
            ErrorItems = errorItemCount,
            TotalAmount = totalAmount,
            Currency = dto.Items.FirstOrDefault()?.Currency ?? "USD"
        };

        response.Errors = errors;
        response.Warnings = warnings;

        return response;
    }

    /// <summary>
    /// Import phiếu nhập kho - Tạo/cập nhật MaterialItems và tạo Receipt records
    /// </summary>
    public async Task<MaterialReceiptResponseDto> ImportReceiptAsync(ImportReceiptDto dto)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        
        try
        {
            // 1. Tạo phiếu nhập (Draft status)
            var receipt = new MaterialReceipt
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

            await _context.MaterialReceipts.AddAsync(receipt);
            await _context.SaveChangesAsync();

            decimal totalAmount = 0;
            var receiptItems = new List<MaterialReceiptItem>();
            var processedItems = new List<MaterialReceiptItemResponseDto>();
            
            // Load categories for lookup
            var categories = await _context.MaterialCategories
                .Where(c => c.IsActive)
                .ToListAsync();

            // 2. Xử lý từng item
            foreach (var itemDto in dto.Items)
            {
                try
                {
                    // 2a. Tìm hoặc tạo MaterialItem
                    var materialItem = await _context.MaterialItems
                        .FirstOrDefaultAsync(m => m.ItemCode == itemDto.ItemCode);

                    bool isNewItem = false;

                    if (materialItem == null)
                    {
                        // TẠO MỚI MaterialItem
                        isNewItem = true;

                        // Tìm category
                        long? categoryId = itemDto.CategoryId;
                        if (!categoryId.HasValue && !string.IsNullOrEmpty(itemDto.CategoryName))
                        {
                            var category = await _context.MaterialCategories
                                .FirstOrDefaultAsync(c => c.Name == itemDto.CategoryName && c.IsActive);
                            categoryId = category?.Id;
                        }

                        materialItem = new MaterialItem
                        {
                            ItemCode = itemDto.ItemCode,
                            Name = itemDto.ItemName,
                            CategoryId = categoryId ?? 1, // Default category if not found
                            Unit = itemDto.Unit,
                            OnHandQuantity = (double)itemDto.Quantity, // Cast decimal to double
                            UnitCost = itemDto.UnitCost,
                            Currency = itemDto.Currency ?? "USD",
                            Location = itemDto.Location,
                            PartNumber = itemDto.PartNumber,
                            Barcode = itemDto.Barcode,
                            Manufacturer = itemDto.Manufacturer,
                            Specification = itemDto.Specification,
                            MinStock = (double?)itemDto.MinStock, // Cast decimal? to double?
                            MaxStock = (double?)itemDto.MaxStock, // Cast decimal? to double?
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };

                        await _context.MaterialItems.AddAsync(materialItem);
                        await _context.SaveChangesAsync();

                        _logger.LogInformation($"Created new MaterialItem: {materialItem.ItemCode} - {materialItem.Name}");
                    }
                    else
                    {
                        // CẬP NHẬT số lượng MaterialItem có sẵn
                        materialItem.OnHandQuantity += (double)itemDto.Quantity; // Cast decimal to double
                        
                        // Cập nhật giá nếu có
                        if (itemDto.UnitCost.HasValue)
                        {
                            materialItem.UnitCost = itemDto.UnitCost;
                        }
                        
                        // Cập nhật location nếu có
                        if (!string.IsNullOrEmpty(itemDto.Location))
                        {
                            materialItem.Location = itemDto.Location;
                        }

                        materialItem.UpdatedAt = DateTime.UtcNow;

                        _logger.LogInformation($"Updated MaterialItem: {materialItem.ItemCode} - Added quantity: {itemDto.Quantity}");
                    }

                    // 2b. Tạo MaterialReceiptItem (chi tiết phiếu nhập)
                    var receiptItem = new MaterialReceiptItem
                    {
                        ReceiptId = receipt.Id,
                        MaterialItemId = materialItem.Id,
                        Quantity = itemDto.Quantity,
                        UnitCost = itemDto.UnitCost,
                        TotalCost = itemDto.Quantity * (itemDto.UnitCost ?? 0),
                        Currency = itemDto.Currency ?? "USD",
                        LineNumber = itemDto.LineNumber,
                        BatchNumber = itemDto.BatchNumber,
                        ExpiryDate = itemDto.ExpiryDate,
                        Notes = itemDto.Notes,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    await _context.MaterialReceiptItems.AddAsync(receiptItem);
                    totalAmount += receiptItem.TotalCost ?? 0;

                    // Lookup category name for response
                    string categoryName = "Unknown";
                    if (categories.Any(c => c.Id == materialItem.CategoryId))
                    {
                        categoryName = categories.First(c => c.Id == materialItem.CategoryId).Name;
                    }

                    processedItems.Add(new MaterialReceiptItemResponseDto
                    {
                        Id = receiptItem.Id,
                        LineNumber = receiptItem.LineNumber ?? 0,
                        MaterialItemId = materialItem.Id,
                        ItemCode = materialItem.ItemCode,
                        ItemName = materialItem.Name,
                        CategoryName = categoryName,
                        Quantity = receiptItem.Quantity,
                        Unit = materialItem.Unit,
                        UnitCost = receiptItem.UnitCost,
                        TotalCost = receiptItem.TotalCost,
                        Currency = receiptItem.Currency,
                        BatchNumber = receiptItem.BatchNumber,
                        ExpiryDate = receiptItem.ExpiryDate,
                        Notes = receiptItem.Notes
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error processing item: {itemDto.ItemCode}");
                    throw;
                }
            }

            // 3. Cập nhật tổng giá trị phiếu và đổi status
            receipt.TotalAmount = totalAmount;
            receipt.Status = "Completed";
            receipt.ApprovedDate = DateTime.UtcNow;
            receipt.Currency = dto.Items.FirstOrDefault()?.Currency ?? "USD";

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation($"Successfully imported receipt {receipt.ReceiptCode} with {processedItems.Count} items");

            // 4. Return response
            return new MaterialReceiptResponseDto
            {
                Id = receipt.Id,
                ReceiptCode = receipt.ReceiptCode,
                ReceiptDate = receipt.ReceiptDate,
                TotalAmount = receipt.TotalAmount,
                Currency = receipt.Currency,
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
            _logger.LogError(ex, "Failed to import receipt");
            throw new Exception($"Import failed: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Lấy danh sách phiếu nhập có phân trang
    /// </summary>
    public async Task<MaterialReceiptPagedResponseDto> GetReceiptsAsync(ReceiptQueryDto query)
    {
        var receiptsQuery = _context.MaterialReceipts
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

        var receiptDtos = receipts.Select(r => new MaterialReceiptListDto
        {
            Id = r.Id,
            ReceiptCode = r.ReceiptCode,
            ReceiptDate = r.ReceiptDate,
            TotalAmount = r.TotalAmount,
            Currency = r.Currency,
            Status = r.Status,
            ItemCount = r.ReceiptItems.Count,
            CreatedBy = r.CreatedBy,
            CreatedAt = r.CreatedAt
        }).ToList();

        return new MaterialReceiptPagedResponseDto
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
    public async Task<MaterialReceiptResponseDto?> GetReceiptByIdAsync(int id)
    {
        var receipt = await _context.MaterialReceipts
            .Include(r => r.ReceiptItems)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (receipt == null)
            return null;
            
        // Load MaterialItems manually
        var materialItemIds = receipt.ReceiptItems.Select(ri => ri.MaterialItemId).Distinct().ToList();
        var materialItems = await _context.MaterialItems
            .Where(m => materialItemIds.Contains(m.Id))
            .ToListAsync();
        var materialItemDict = materialItems.ToDictionary(m => m.Id);
        
        // Load Categories
        var categoryIds = materialItems.Select(m => m.CategoryId).Distinct().ToList();
        var categories = await _context.MaterialCategories
            .Where(c => categoryIds.Contains(c.Id))
            .ToListAsync();
        var categoryDict = categories.ToDictionary(c => c.Id, c => c.Name);

        return new MaterialReceiptResponseDto
        {
            Id = receipt.Id,
            ReceiptCode = receipt.ReceiptCode,
            ReceiptDate = receipt.ReceiptDate,
            TotalAmount = receipt.TotalAmount,
            Currency = receipt.Currency,
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
                var mat = materialItemDict.TryGetValue(ri.MaterialItemId, out var m) ? m : null;
                return new MaterialReceiptItemResponseDto
                {
                    Id = ri.Id,
                    LineNumber = ri.LineNumber ?? 0,
                    MaterialItemId = ri.MaterialItemId,
                    ItemCode = mat?.ItemCode ?? "",
                    ItemName = mat?.Name ?? "",
                    CategoryName = mat != null && categoryDict.TryGetValue(mat.CategoryId, out var catName) 
                        ? catName 
                        : "",
                    Quantity = ri.Quantity,
                    Unit = mat?.Unit ?? "",
                    UnitCost = ri.UnitCost,
                    TotalCost = ri.TotalCost,
                    Currency = ri.Currency,
                    BatchNumber = ri.BatchNumber,
                    ExpiryDate = ri.ExpiryDate,
                    Notes = ri.Notes
                };
            }).ToList()
        };
    }

    /// <summary>
    /// Generate mã phiếu nhập tự động (Format: PN-YYYYMMDD-XXX)
    /// </summary>
    private async Task<string> GenerateReceiptCodeAsync()
    {
        var today = DateTime.Today;
        var datePrefix = $"PN-{today:yyyyMMdd}";

        // Lấy số sequence cuối cùng trong ngày
        var lastReceipt = await _context.MaterialReceipts
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
