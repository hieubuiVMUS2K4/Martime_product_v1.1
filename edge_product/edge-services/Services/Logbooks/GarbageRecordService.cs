using MaritimeEdge.Data;
using MaritimeEdge.DTOs.Logbooks;
using MaritimeEdge.Models;
using MaritimeEdge.Services.Voyage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MaritimeEdge.Services.Logbooks
{
    public class GarbageRecordService : IGarbageRecordService
    {
        private readonly EdgeDbContext _context;
        private readonly ILogger<GarbageRecordService> _logger;
        private readonly IVoyageContextService _voyageContext;

        public GarbageRecordService(EdgeDbContext context, ILogger<GarbageRecordService> logger, IVoyageContextService voyageContext)
        {
            _context = context;
            _logger = logger;
            _voyageContext = voyageContext;
        }

        public async Task<(bool Success, Guid? Id, string? Error)> CreateEntryAsync(CreateGarbageRecordEntryDto dto, string? username = null)
        {
            try
            {
                var entry = new GarbageRecordBook
                {
                    Id = Guid.NewGuid(),
                    OperationDateTime = dto.OperationDateTime,
                    OperationCode = dto.OperationCode,
                    GarbageCategory = dto.GarbageCategory,
                    Description = dto.Description,
                    Quantity = dto.Quantity,
                    QuantityUnit = dto.QuantityUnit,
                    Latitude = dto.Latitude,
                    Longitude = dto.Longitude,
                    PortName = dto.PortName,
                    ReceptionFacility = dto.ReceptionFacility,
                    ReceiptNumber = dto.ReceiptNumber,
                    IncinerationStartTime = dto.IncinerationStartTime,
                    IncinerationEndTime = dto.IncinerationEndTime,
                    IncineratorDetails = dto.IncineratorDetails,
                    AccidentalDischargeReason = dto.AccidentalDischargeReason,
                    AccidentalDischargeMeasures = dto.AccidentalDischargeMeasures,
                    OfficerInCharge = dto.OfficerInCharge,
                    Remarks = dto.Remarks,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    OriginNode = Environment.MachineName,
                    IsSynced = false
                };

                var (voyageId, legId) = await _voyageContext.ResolveActiveVoyageAsync(entry.OperationDateTime);
                entry.VoyageId = voyageId;
                entry.VoyagePlanLegId = legId;

                _context.GarbageRecordBooks.Add(entry);
                await _context.SaveChangesAsync();

                return (true, entry.Id, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating garbage record entry");
                return (false, null, "An error occurred while creating the entry.");
            }
        }

        public async Task<(bool Success, string? Error)> DeleteEntryAsync(Guid id, string? username = null)
        {
            try
            {
                var entry = await _context.GarbageRecordBooks.FindAsync(id);
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
                _logger.LogError(ex, "Error deleting garbage record entry {Id}", id);
                return (false, "An error occurred while deleting the entry.");
            }
        }

        public async Task<PaginatedLogbookResponseDto<GarbageRecordEntryResponseDto>> GetEntriesAsync(LogbookPaginationDto pagination)
        {
            try
            {
                var query = _context.GarbageRecordBooks.AsQueryable();

                // Filter out soft-deleted items
                query = query.Where(x => !x.IsDeleted);

                if (pagination.FromDate.HasValue)
                {
                    query = query.Where(x => x.OperationDateTime >= pagination.FromDate.Value);
                }

                if (pagination.ToDate.HasValue)
                {
                    query = query.Where(x => x.OperationDateTime <= pagination.ToDate.Value);
                }

                if (!string.IsNullOrEmpty(pagination.SearchTerm))
                {
                    var term = pagination.SearchTerm.ToLower();
                    query = query.Where(x => 
                        x.OfficerInCharge.ToLower().Contains(term) || 
                        x.Description.ToLower().Contains(term) ||
                        x.GarbageCategory.ToLower().Contains(term)
                    );
                }

                var totalCount = await query.CountAsync();

                query = query.OrderByDescending(x => x.OperationDateTime)
                             .ThenByDescending(x => x.CreatedAt);

                var items = await query
                    .Skip((pagination.Page - 1) * pagination.PageSize)
                    .Take(pagination.PageSize)
                    .Select(x => new GarbageRecordEntryResponseDto
                    {
                        Id = x.Id,
                        OperationDateTime = x.OperationDateTime,
                        OperationCode = x.OperationCode,
                        GarbageCategory = x.GarbageCategory,
                        Description = x.Description,
                        Quantity = x.Quantity,
                        QuantityUnit = x.QuantityUnit,
                        Latitude = x.Latitude,
                        Longitude = x.Longitude,
                        PortName = x.PortName,
                        ReceptionFacility = x.ReceptionFacility,
                        ReceiptNumber = x.ReceiptNumber,
                        IncinerationStartTime = x.IncinerationStartTime,
                        IncinerationEndTime = x.IncinerationEndTime,
                        IncineratorDetails = x.IncineratorDetails,
                        AccidentalDischargeReason = x.AccidentalDischargeReason,
                        AccidentalDischargeMeasures = x.AccidentalDischargeMeasures,
                        OfficerInCharge = x.OfficerInCharge,
                        MasterSignature = x.MasterSignature,
                        SignedAt = x.SignedAt,
                        Remarks = x.Remarks,
                        IsSynced = x.IsSynced,
                        CreatedAt = x.CreatedAt,
                        UpdatedAt = x.UpdatedAt,
                        OriginNode = x.OriginNode,
                        VoyageId = x.VoyageId,
                        VoyagePlanLegId = x.VoyagePlanLegId
                    })
                    .ToListAsync();

                return new PaginatedLogbookResponseDto<GarbageRecordEntryResponseDto>
                {
                    Data = items,
                    TotalRecords = totalCount,
                    Page = pagination.Page,
                    PageSize = pagination.PageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving garbage record entries");
                throw;
            }
        }

        public async Task<GarbageRecordEntryResponseDto?> GetEntryAsync(Guid id)
        {
            try
            {
                var entry = await _context.GarbageRecordBooks
                    .Where(x => x.Id == id && !x.IsDeleted)
                    .FirstOrDefaultAsync();

                if (entry == null) return null;

                return new GarbageRecordEntryResponseDto
                {
                    Id = entry.Id,
                    OperationDateTime = entry.OperationDateTime,
                    OperationCode = entry.OperationCode,
                    GarbageCategory = entry.GarbageCategory,
                    Description = entry.Description,
                    Quantity = entry.Quantity,
                    QuantityUnit = entry.QuantityUnit,
                    Latitude = entry.Latitude,
                    Longitude = entry.Longitude,
                    PortName = entry.PortName,
                    ReceptionFacility = entry.ReceptionFacility,
                    ReceiptNumber = entry.ReceiptNumber,
                    IncinerationStartTime = entry.IncinerationStartTime,
                    IncinerationEndTime = entry.IncinerationEndTime,
                    IncineratorDetails = entry.IncineratorDetails,
                    AccidentalDischargeReason = entry.AccidentalDischargeReason,
                    AccidentalDischargeMeasures = entry.AccidentalDischargeMeasures,
                    OfficerInCharge = entry.OfficerInCharge,
                    MasterSignature = entry.MasterSignature,
                    SignedAt = entry.SignedAt,
                    Remarks = entry.Remarks,
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
                _logger.LogError(ex, "Error retrieving garbage record entry {Id}", id);
                throw;
            }
        }

        public async Task<(bool Success, string? Error)> SignEntryAsync(Guid id, SignGarbageRecordDto dto, string? username = null)
        {
            try
            {
                var entry = await _context.GarbageRecordBooks.FindAsync(id);
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
                _logger.LogError(ex, "Error signing garbage record entry {Id}", id);
                return (false, "An error occurred while signing the entry.");
            }
        }

        public async Task<(bool Success, string? Error)> UpdateEntryAsync(Guid id, UpdateGarbageRecordEntryDto dto, string? username = null)
        {
            try
            {
                var entry = await _context.GarbageRecordBooks.FindAsync(id);
                if (entry == null || entry.IsDeleted)
                {
                    return (false, "Entry not found.");
                }

                if (!string.IsNullOrEmpty(entry.MasterSignature))
                {
                    return (false, "Cannot update a signed entry.");
                }

                // Required fields - always update
                entry.OperationDateTime = dto.OperationDateTime;
                entry.OperationCode = dto.OperationCode;
                entry.GarbageCategory = dto.GarbageCategory;
                entry.Description = dto.Description;
                entry.Quantity = dto.Quantity;
                entry.OfficerInCharge = dto.OfficerInCharge;

                // Non-required fields with defaults
                entry.QuantityUnit = dto.QuantityUnit;

                // Nullable fields - only update if provided
                if (dto.Latitude.HasValue) entry.Latitude = dto.Latitude;
                if (dto.Longitude.HasValue) entry.Longitude = dto.Longitude;
                if (dto.PortName != null) entry.PortName = dto.PortName;
                if (dto.ReceptionFacility != null) entry.ReceptionFacility = dto.ReceptionFacility;
                if (dto.ReceiptNumber != null) entry.ReceiptNumber = dto.ReceiptNumber;
                if (dto.IncinerationStartTime.HasValue) entry.IncinerationStartTime = dto.IncinerationStartTime;
                if (dto.IncinerationEndTime.HasValue) entry.IncinerationEndTime = dto.IncinerationEndTime;
                if (dto.IncineratorDetails != null) entry.IncineratorDetails = dto.IncineratorDetails;
                if (dto.AccidentalDischargeReason != null) entry.AccidentalDischargeReason = dto.AccidentalDischargeReason;
                if (dto.AccidentalDischargeMeasures != null) entry.AccidentalDischargeMeasures = dto.AccidentalDischargeMeasures;
                if (dto.Remarks != null) entry.Remarks = dto.Remarks;
                
                entry.UpdatedAt = DateTime.UtcNow;
                entry.IsSynced = false;

                await _context.SaveChangesAsync();
                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating garbage record entry {Id}", id);
                return (false, "An error occurred while updating the entry.");
            }
        }
    }
}
