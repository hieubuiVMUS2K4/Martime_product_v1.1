using MaritimeEdge.Data;
using MaritimeEdge.DTOs.Logbooks;
using MaritimeEdge.Models;
using MaritimeEdge.Services.Voyage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MaritimeEdge.Services.Logbooks
{
    public class GarbagePartIIService : IGarbagePartIIService
    {
        private readonly EdgeDbContext _context;
        private readonly ILogger<GarbagePartIIService> _logger;
        private readonly IVoyageContextService _voyageContext;

        public GarbagePartIIService(EdgeDbContext context, ILogger<GarbagePartIIService> logger, IVoyageContextService voyageContext)
        {
            _context = context;
            _logger = logger;
            _voyageContext = voyageContext;
        }

        public async Task<(bool Success, Guid? Id, string? Error)> CreateEntryAsync(CreateGarbagePartIIDto dto, string? username = null)
        {
            try
            {
                // Validate category
                if (!IsValidPartIICategory(dto.Category))
                {
                    return (false, null, "Invalid category for Part II. Must be J or K.");
                }

                // CRITICAL: Category K (HME) CANNOT be discharged to sea
                if (dto.Category.ToUpper() == "K" && (dto.EstimatedAmountDischargedToSea ?? 0) > 0)
                {
                    return (false, null, "MARPOL VIOLATION: Category K (HME cargo residues) CANNOT be discharged to sea. Must be discharged to reception facilities only.");
                }

                // Category K MUST have reception facility discharge
                if (dto.Category.ToUpper() == "K" && (dto.EstimatedAmountToReceptionFacilities ?? 0) <= 0)
                {
                    return (false, null, "Category K (HME) must be discharged to reception facilities. Sea discharge is strictly prohibited.");
                }

                // Validate amounts
                var validationError = ValidateAmounts(dto);
                if (validationError != null)
                {
                    return (false, null, validationError);
                }

                // Validate reception facility details when present
                if ((dto.EstimatedAmountToReceptionFacilities ?? 0) > 0)
                {
                    if (string.IsNullOrEmpty(dto.PortName) && string.IsNullOrEmpty(dto.ReceptionFacilityName))
                    {
                        return (false, null, "Port name or reception facility name is required for discharge to reception facility.");
                    }
                }

                var entry = new GarbageRecordPartII
                {
                    Id = Guid.NewGuid(),
                    OperationDate = dto.OperationDate.Date,
                    OperationTime = dto.OperationTime,
                    OperationEndTime = dto.OperationEndTime,
                    Category = dto.Category.ToUpper(),
                    StartLatitude = dto.StartLatitude,
                    StartLongitude = dto.StartLongitude,
                    EndLatitude = dto.EndLatitude,
                    EndLongitude = dto.EndLongitude,
                    EstimatedAmountDischargedToSea = dto.EstimatedAmountDischargedToSea,
                    EstimatedAmountToReceptionFacilities = dto.EstimatedAmountToReceptionFacilities,
                    PortName = dto.PortName,
                    ReceptionFacilityName = dto.ReceptionFacilityName,
                    ReceiptNumber = dto.ReceiptNumber,
                    CargoDescription = dto.CargoDescription,
                    HoldNumbersWashed = dto.HoldNumbersWashed,
                    Remarks = dto.Remarks,
                    OfficerInCharge = dto.OfficerInCharge,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    OriginNode = Environment.MachineName,
                    IsSynced = false
                };

                var (voyageId, legId) = await _voyageContext.ResolveActiveVoyageAsync(entry.OperationDate);
                entry.VoyageId = voyageId;
                entry.VoyagePlanLegId = legId;

                _context.GarbageRecordPartIIs.Add(entry);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created Garbage Part II entry {Id} for category {Category}", entry.Id, entry.Category);
                return (true, entry.Id, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Garbage Part II entry");
                return (false, null, "An error occurred while creating the entry.");
            }
        }

        public async Task<(bool Success, string? Error)> DeleteEntryAsync(Guid id, string? username = null)
        {
            try
            {
                var entry = await _context.GarbageRecordPartIIs.FindAsync(id);
                if (entry == null)
                {
                    return (false, "Entry not found.");
                }

                if (entry.IsDeleted)
                {
                    return (false, "Entry is already deleted.");
                }

                // Soft delete
                entry.IsDeleted = true;
                entry.DeletedAt = DateTime.UtcNow;
                entry.DeletedBy = username ?? "System";
                entry.UpdatedAt = DateTime.UtcNow;
                entry.IsSynced = false;

                await _context.SaveChangesAsync();
                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting Garbage Part II entry {Id}", id);
                return (false, "An error occurred while deleting the entry.");
            }
        }

        public async Task<PaginatedLogbookResponseDto<GarbagePartIIResponseDto>> GetEntriesAsync(LogbookPaginationDto pagination)
        {
            try
            {
                var query = _context.GarbageRecordPartIIs.Where(x => !x.IsDeleted);

                if (pagination.FromDate.HasValue)
                {
                    query = query.Where(x => x.OperationDate >= pagination.FromDate.Value.Date);
                }

                if (pagination.ToDate.HasValue)
                {
                    query = query.Where(x => x.OperationDate <= pagination.ToDate.Value.Date);
                }

                if (!string.IsNullOrEmpty(pagination.SearchTerm))
                {
                    var term = pagination.SearchTerm.ToLower();
                    query = query.Where(x =>
                        x.OfficerInCharge.ToLower().Contains(term) ||
                        x.CargoDescription.ToLower().Contains(term) ||
                        x.Category.ToLower().Contains(term));
                }

                var totalCount = await query.CountAsync();

                var items = await query
                    .OrderByDescending(x => x.OperationDate)
                    .ThenByDescending(x => x.OperationTime)
                    .Skip((pagination.Page - 1) * pagination.PageSize)
                    .Take(pagination.PageSize)
                    .Select(x => new GarbagePartIIResponseDto
                    {
                        Id = x.Id,
                        OperationDate = x.OperationDate,
                        OperationTime = x.OperationTime,
                        Category = x.Category,
                        StartLatitude = x.StartLatitude,
                        StartLongitude = x.StartLongitude,
                        EndLatitude = x.EndLatitude,
                        EndLongitude = x.EndLongitude,
                        EstimatedAmountDischargedToSea = x.EstimatedAmountDischargedToSea,
                        EstimatedAmountToReceptionFacilities = x.EstimatedAmountToReceptionFacilities,
                        PortName = x.PortName,
                        ReceptionFacilityName = x.ReceptionFacilityName,
                        ReceiptNumber = x.ReceiptNumber,
                        CargoDescription = x.CargoDescription,
                        HoldNumbersWashed = x.HoldNumbersWashed,
                        Remarks = x.Remarks,
                        OfficerInCharge = x.OfficerInCharge,
                        MasterSignature = x.MasterSignature,
                        SignedAt = x.SignedAt,
                        IsSynced = x.IsSynced,
                        CreatedAt = x.CreatedAt,
                        UpdatedAt = x.UpdatedAt,
                        OriginNode = x.OriginNode,
                        VoyageId = x.VoyageId,
                        VoyagePlanLegId = x.VoyagePlanLegId
                    })
                    .ToListAsync();

                return new PaginatedLogbookResponseDto<GarbagePartIIResponseDto>
                {
                    Data = items,
                    TotalRecords = totalCount,
                    Page = pagination.Page,
                    PageSize = pagination.PageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Garbage Part II entries");
                throw;
            }
        }

        public async Task<GarbagePartIIResponseDto?> GetEntryAsync(Guid id)
        {
            try
            {
                var entry = await _context.GarbageRecordPartIIs
                    .Where(x => x.Id == id && !x.IsDeleted)
                    .FirstOrDefaultAsync();

                if (entry == null) return null;

                return new GarbagePartIIResponseDto
                {
                    Id = entry.Id,
                    OperationDate = entry.OperationDate,
                    OperationTime = entry.OperationTime,
                    Category = entry.Category,
                    StartLatitude = entry.StartLatitude,
                    StartLongitude = entry.StartLongitude,
                    EndLatitude = entry.EndLatitude,
                    EndLongitude = entry.EndLongitude,
                    EstimatedAmountDischargedToSea = entry.EstimatedAmountDischargedToSea,
                    EstimatedAmountToReceptionFacilities = entry.EstimatedAmountToReceptionFacilities,
                    PortName = entry.PortName,
                    ReceptionFacilityName = entry.ReceptionFacilityName,
                    ReceiptNumber = entry.ReceiptNumber,
                    CargoDescription = entry.CargoDescription,
                    HoldNumbersWashed = entry.HoldNumbersWashed,
                    Remarks = entry.Remarks,
                    OfficerInCharge = entry.OfficerInCharge,
                    MasterSignature = entry.MasterSignature,
                    SignedAt = entry.SignedAt,
                    IsSynced = entry.IsSynced,
                    CreatedAt = entry.CreatedAt,
                    UpdatedAt = entry.UpdatedAt,
                    OriginNode = entry.OriginNode,
                    VoyageId = entry.VoyageId,
                    VoyagePlanLegId = entry.VoyagePlanLegId
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Garbage Part II entry {Id}", id);
                throw;
            }
        }

        public async Task<(bool Success, string? Error)> SignEntryAsync(Guid id, SignGarbagePartIIDto dto, string? username = null)
        {
            try
            {
                var entry = await _context.GarbageRecordPartIIs.FindAsync(id);
                if (entry == null || entry.IsDeleted)
                {
                    return (false, "Entry not found.");
                }

                entry.MasterSignature = dto.MasterSignature;
                entry.SignedAt = dto.SignedAt;
                entry.UpdatedAt = DateTime.UtcNow;
                entry.IsSynced = false;

                await _context.SaveChangesAsync();
                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error signing Garbage Part II entry {Id}", id);
                return (false, "An error occurred while signing the entry.");
            }
        }

        public async Task<(bool Success, string? Error)> UpdateEntryAsync(Guid id, UpdateGarbagePartIIDto dto, string? username = null)
        {
            try
            {
                var entry = await _context.GarbageRecordPartIIs.FindAsync(id);
                if (entry == null || entry.IsDeleted)
                {
                    return (false, "Entry not found.");
                }

                if (!string.IsNullOrEmpty(entry.MasterSignature))
                {
                    return (false, "Cannot update a signed entry.");
                }

                // Validate
                if (!IsValidPartIICategory(dto.Category))
                {
                    return (false, "Invalid category for Part II. Must be J or K.");
                }

                // CRITICAL: Category K validation
                if (dto.Category.ToUpper() == "K" && (dto.EstimatedAmountDischargedToSea ?? 0) > 0)
                {
                    return (false, "MARPOL VIOLATION: Category K (HME) cannot be discharged to sea.");
                }

                if (dto.Category.ToUpper() == "K" && (dto.EstimatedAmountToReceptionFacilities ?? 0) <= 0)
                {
                    return (false, "Category K (HME) must be discharged to reception facilities.");
                }

                var validationError = ValidateAmounts(dto);
                if (validationError != null)
                {
                    return (false, validationError);
                }

                // Required fields - always update
                entry.OperationDate = dto.OperationDate.Date;
                entry.OperationTime = dto.OperationTime;
                entry.Category = dto.Category.ToUpper();
                entry.StartLatitude = dto.StartLatitude;
                entry.StartLongitude = dto.StartLongitude;
                entry.EndLatitude = dto.EndLatitude;
                entry.EndLongitude = dto.EndLongitude;
                entry.CargoDescription = dto.CargoDescription;
                entry.HoldNumbersWashed = dto.HoldNumbersWashed;
                entry.OfficerInCharge = dto.OfficerInCharge;

                // Nullable fields - only update if provided
                if (dto.OperationEndTime.HasValue) entry.OperationEndTime = dto.OperationEndTime;
                if (dto.EstimatedAmountDischargedToSea.HasValue) entry.EstimatedAmountDischargedToSea = dto.EstimatedAmountDischargedToSea;
                if (dto.EstimatedAmountToReceptionFacilities.HasValue) entry.EstimatedAmountToReceptionFacilities = dto.EstimatedAmountToReceptionFacilities;
                if (dto.PortName != null) entry.PortName = dto.PortName;
                if (dto.ReceptionFacilityName != null) entry.ReceptionFacilityName = dto.ReceptionFacilityName;
                if (dto.ReceiptNumber != null) entry.ReceiptNumber = dto.ReceiptNumber;
                if (dto.Remarks != null) entry.Remarks = dto.Remarks;
                entry.UpdatedAt = DateTime.UtcNow;
                entry.IsSynced = false;

                await _context.SaveChangesAsync();
                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating Garbage Part II entry {Id}", id);
                return (false, "An error occurred while updating the entry.");
            }
        }

        private bool IsValidPartIICategory(string category)
        {
            var validCategories = new[] { "J", "K" };
            return validCategories.Contains(category.ToUpper());
        }

        private string? ValidateAmounts(CreateGarbagePartIIDto dto)
        {
            var totalAmount = (dto.EstimatedAmountDischargedToSea ?? 0) +
                            (dto.EstimatedAmountToReceptionFacilities ?? 0);

            if (totalAmount <= 0)
            {
                return "At least one amount (sea/reception) must be greater than 0.";
            }

            return null;
        }
    }
}
