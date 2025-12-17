using MaritimeEdge.Data;
using MaritimeEdge.DTOs.Logbooks;
using MaritimeEdge.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MaritimeEdge.Services.Logbooks
{
    public class OilRecordService : IOilRecordService
    {
        private readonly EdgeDbContext _context;
        private readonly ILogger<OilRecordService> _logger;

        public OilRecordService(EdgeDbContext context, ILogger<OilRecordService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<(bool Success, Guid? Id, string? Error)> CreateEntryAsync(CreateOilRecordEntryDto dto, string? username = null)
        {
            try
            {
                var entry = new OilRecordBook
                {
                    Id = Guid.NewGuid(),
                    EntryDate = dto.EntryDate,
                    OperationCode = dto.OperationCode,
                    OperationDescription = dto.OperationDescription,
                    LocationLat = dto.LocationLat,
                    LocationLon = dto.LocationLon,
                    Quantity = dto.Quantity,
                    QuantityUnit = dto.QuantityUnit,
                    TankFrom = dto.TankFrom,
                    TankTo = dto.TankTo,
                    OfficerInCharge = dto.OfficerInCharge,
                    Remarks = dto.Remarks,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    OriginNode = Environment.MachineName,
                    IsSynced = false
                };

                _context.OilRecordBooks.Add(entry);
                await _context.SaveChangesAsync();

                return (true, entry.Id, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating oil record entry");
                return (false, null, "An error occurred while creating the entry.");
            }
        }

        public async Task<(bool Success, string? Error)> DeleteEntryAsync(Guid id, string? username = null)
        {
            try
            {
                var entry = await _context.OilRecordBooks.FindAsync(id);
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
                _logger.LogError(ex, "Error deleting oil record entry {Id}", id);
                return (false, "An error occurred while deleting the entry.");
            }
        }

        public async Task<PaginatedLogbookResponseDto<OilRecordEntryResponseDto>> GetEntriesAsync(LogbookPaginationDto pagination)
        {
            try
            {
                var query = _context.OilRecordBooks.AsQueryable();

                // Filter out soft-deleted items
                query = query.Where(x => !x.IsDeleted);

                if (pagination.FromDate.HasValue)
                {
                    query = query.Where(x => x.EntryDate >= pagination.FromDate.Value);
                }

                if (pagination.ToDate.HasValue)
                {
                    query = query.Where(x => x.EntryDate <= pagination.ToDate.Value);
                }

                if (!string.IsNullOrEmpty(pagination.SearchTerm))
                {
                    var term = pagination.SearchTerm.ToLower();
                    query = query.Where(x => 
                        x.OfficerInCharge.ToLower().Contains(term) || 
                        x.OperationDescription.ToLower().Contains(term) ||
                        x.OperationCode.ToLower().Contains(term)
                    );
                }

                var totalCount = await query.CountAsync();

                query = query.OrderByDescending(x => x.EntryDate)
                             .ThenByDescending(x => x.CreatedAt);

                var items = await query
                    .Skip((pagination.Page - 1) * pagination.PageSize)
                    .Take(pagination.PageSize)
                    .Select(x => new OilRecordEntryResponseDto
                    {
                        Id = x.Id,
                        EntryDate = x.EntryDate,
                        OperationCode = x.OperationCode,
                        OperationDescription = x.OperationDescription,
                        LocationLat = x.LocationLat,
                        LocationLon = x.LocationLon,
                        Quantity = x.Quantity,
                        QuantityUnit = x.QuantityUnit,
                        TankFrom = x.TankFrom,
                        TankTo = x.TankTo,
                        OfficerInCharge = x.OfficerInCharge,
                        MasterSignature = x.MasterSignature,
                        Remarks = x.Remarks,
                        IsSynced = x.IsSynced,
                        CreatedAt = x.CreatedAt,
                        UpdatedAt = x.UpdatedAt,
                        OriginNode = x.OriginNode
                    })
                    .ToListAsync();

                return new PaginatedLogbookResponseDto<OilRecordEntryResponseDto>
                {
                    Data = items,
                    TotalRecords = totalCount,
                    Page = pagination.Page,
                    PageSize = pagination.PageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving oil record entries");
                throw;
            }
        }

        public async Task<OilRecordEntryResponseDto?> GetEntryAsync(Guid id)
        {
            try
            {
                var entry = await _context.OilRecordBooks
                    .Where(x => x.Id == id && !x.IsDeleted)
                    .FirstOrDefaultAsync();

                if (entry == null) return null;

                return new OilRecordEntryResponseDto
                {
                    Id = entry.Id,
                    EntryDate = entry.EntryDate,
                    OperationCode = entry.OperationCode,
                    OperationDescription = entry.OperationDescription,
                    LocationLat = entry.LocationLat,
                    LocationLon = entry.LocationLon,
                    Quantity = entry.Quantity,
                    QuantityUnit = entry.QuantityUnit,
                    TankFrom = entry.TankFrom,
                    TankTo = entry.TankTo,
                    OfficerInCharge = entry.OfficerInCharge,
                    MasterSignature = entry.MasterSignature,
                    Remarks = entry.Remarks,
                    IsSynced = entry.IsSynced,
                    CreatedAt = entry.CreatedAt,
                    UpdatedAt = entry.UpdatedAt,
                    OriginNode = entry.OriginNode
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving oil record entry {Id}", id);
                throw;
            }
        }

        public async Task<(bool Success, string? Error)> SignEntryAsync(Guid id, SignOilRecordDto dto, string? username = null)
        {
            try
            {
                var entry = await _context.OilRecordBooks.FindAsync(id);
                if (entry == null || entry.IsDeleted)
                {
                    return (false, "Entry not found.");
                }

                entry.MasterSignature = dto.MasterSignature;
                entry.UpdatedAt = DateTime.UtcNow;
                entry.IsSynced = false;

                await _context.SaveChangesAsync();
                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error signing oil record entry {Id}", id);
                return (false, "An error occurred while signing the entry.");
            }
        }

        public async Task<(bool Success, string? Error)> UpdateEntryAsync(Guid id, UpdateOilRecordEntryDto dto, string? username = null)
        {
            try
            {
                var entry = await _context.OilRecordBooks.FindAsync(id);
                if (entry == null || entry.IsDeleted)
                {
                    return (false, "Entry not found.");
                }

                if (!string.IsNullOrEmpty(entry.MasterSignature))
                {
                    return (false, "Cannot update a signed entry.");
                }

                entry.EntryDate = dto.EntryDate;
                entry.OperationCode = dto.OperationCode;
                entry.OperationDescription = dto.OperationDescription;
                entry.LocationLat = dto.LocationLat;
                entry.LocationLon = dto.LocationLon;
                entry.Quantity = dto.Quantity;
                entry.QuantityUnit = dto.QuantityUnit;
                entry.TankFrom = dto.TankFrom;
                entry.TankTo = dto.TankTo;
                entry.OfficerInCharge = dto.OfficerInCharge;
                entry.Remarks = dto.Remarks;
                
                entry.UpdatedAt = DateTime.UtcNow;
                entry.IsSynced = false;

                await _context.SaveChangesAsync();
                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating oil record entry {Id}", id);
                return (false, "An error occurred while updating the entry.");
            }
        }
    }
}
