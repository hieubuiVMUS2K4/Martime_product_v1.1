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
    public class BallastWaterService : IBallastWaterService
    {
        private readonly EdgeDbContext _context;
        private readonly ILogger<BallastWaterService> _logger;

        public BallastWaterService(EdgeDbContext context, ILogger<BallastWaterService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<(bool Success, Guid? Id, string? Error)> CreateEntryAsync(CreateBallastWaterEntryDto dto, string? username = null)
        {
            try
            {
                var entry = new BallastWaterRecordBook
                {
                    Id = Guid.NewGuid(),
                    OperationDateTime = dto.OperationDateTime,
                    OperationCode = dto.OperationCode,
                    OperationDescription = dto.OperationDescription,
                    BallastTank = dto.BallastTank,
                    Volume = dto.Volume,
                    StartLatitude = dto.StartLatitude,
                    StartLongitude = dto.StartLongitude,
                    StartDateTime = dto.StartDateTime,
                    EndLatitude = dto.EndLatitude,
                    EndLongitude = dto.EndLongitude,
                    EndDateTime = dto.EndDateTime,
                    WaterDepth = dto.WaterDepth,
                    DistanceFromLand = dto.DistanceFromLand,
                    ExchangeVolumePercentage = dto.ExchangeVolumePercentage,
                    ExchangeMethod = dto.ExchangeMethod,
                    TreatmentSystemUsed = dto.TreatmentSystemUsed,
                    TreatmentSystemType = dto.TreatmentSystemType,
                    TreatmentSuccessful = dto.TreatmentSuccessful,
                    TreatmentDetails = dto.TreatmentDetails,
                    ExceptionalCircumstances = dto.ExceptionalCircumstances,
                    SalinityBeforeExchange = dto.SalinityBeforeExchange,
                    SalinityAfterExchange = dto.SalinityAfterExchange,
                    PortName = dto.PortName,
                    ReceptionFacility = dto.ReceptionFacility,
                    ReceiptNumber = dto.ReceiptNumber,
                    OfficerInCharge = dto.OfficerInCharge,
                    Remarks = dto.Remarks,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    OriginNode = Environment.MachineName,
                    IsSynced = false
                };

                _context.BallastWaterRecordBooks.Add(entry);
                await _context.SaveChangesAsync();

                return (true, entry.Id, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating ballast water record entry");
                return (false, null, "An error occurred while creating the entry.");
            }
        }

        public async Task<(bool Success, string? Error)> DeleteEntryAsync(Guid id, string? username = null)
        {
            try
            {
                var entry = await _context.BallastWaterRecordBooks.FindAsync(id);
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
                _logger.LogError(ex, "Error deleting ballast water record entry {Id}", id);
                return (false, "An error occurred while deleting the entry.");
            }
        }

        public async Task<PaginatedLogbookResponseDto<BallastWaterEntryResponseDto>> GetEntriesAsync(LogbookPaginationDto pagination)
        {
            try
            {
                var query = _context.BallastWaterRecordBooks.AsQueryable();

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
                        x.OperationDescription.ToLower().Contains(term) ||
                        x.BallastTank.ToLower().Contains(term)
                    );
                }

                var totalCount = await query.CountAsync();

                query = query.OrderByDescending(x => x.OperationDateTime)
                             .ThenByDescending(x => x.CreatedAt);

                var items = await query
                    .Skip((pagination.Page - 1) * pagination.PageSize)
                    .Take(pagination.PageSize)
                    .Select(x => new BallastWaterEntryResponseDto
                    {
                        Id = x.Id,
                        OperationDateTime = x.OperationDateTime,
                        OperationCode = x.OperationCode,
                        OperationDescription = x.OperationDescription,
                        BallastTank = x.BallastTank,
                        Volume = x.Volume,
                        StartLatitude = x.StartLatitude,
                        StartLongitude = x.StartLongitude,
                        StartDateTime = x.StartDateTime,
                        EndLatitude = x.EndLatitude,
                        EndLongitude = x.EndLongitude,
                        EndDateTime = x.EndDateTime,
                        WaterDepth = x.WaterDepth,
                        DistanceFromLand = x.DistanceFromLand,
                        ExchangeVolumePercentage = x.ExchangeVolumePercentage,
                        ExchangeMethod = x.ExchangeMethod,
                        TreatmentSystemUsed = x.TreatmentSystemUsed,
                        TreatmentSystemType = x.TreatmentSystemType,
                        TreatmentSuccessful = x.TreatmentSuccessful,
                        TreatmentDetails = x.TreatmentDetails,
                        ExceptionalCircumstances = x.ExceptionalCircumstances,
                        SalinityBeforeExchange = x.SalinityBeforeExchange,
                        SalinityAfterExchange = x.SalinityAfterExchange,
                        PortName = x.PortName,
                        ReceptionFacility = x.ReceptionFacility,
                        ReceiptNumber = x.ReceiptNumber,
                        OfficerInCharge = x.OfficerInCharge,
                        MasterSignature = x.MasterSignature,
                        SignedAt = x.SignedAt,
                        Remarks = x.Remarks,
                        IsSynced = x.IsSynced,
                        CreatedAt = x.CreatedAt,
                        UpdatedAt = x.UpdatedAt,
                        OriginNode = x.OriginNode
                    })
                    .ToListAsync();

                return new PaginatedLogbookResponseDto<BallastWaterEntryResponseDto>
                {
                    Data = items,
                    TotalRecords = totalCount,
                    Page = pagination.Page,
                    PageSize = pagination.PageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving ballast water record entries");
                throw;
            }
        }

        public async Task<BallastWaterEntryResponseDto?> GetEntryAsync(Guid id)
        {
            try
            {
                var entry = await _context.BallastWaterRecordBooks
                    .Where(x => x.Id == id && !x.IsDeleted)
                    .FirstOrDefaultAsync();

                if (entry == null) return null;

                return new BallastWaterEntryResponseDto
                {
                    Id = entry.Id,
                    OperationDateTime = entry.OperationDateTime,
                    OperationCode = entry.OperationCode,
                    OperationDescription = entry.OperationDescription,
                    BallastTank = entry.BallastTank,
                    Volume = entry.Volume,
                    StartLatitude = entry.StartLatitude,
                    StartLongitude = entry.StartLongitude,
                    StartDateTime = entry.StartDateTime,
                    EndLatitude = entry.EndLatitude,
                    EndLongitude = entry.EndLongitude,
                    EndDateTime = entry.EndDateTime,
                    WaterDepth = entry.WaterDepth,
                    DistanceFromLand = entry.DistanceFromLand,
                    ExchangeVolumePercentage = entry.ExchangeVolumePercentage,
                    ExchangeMethod = entry.ExchangeMethod,
                    TreatmentSystemUsed = entry.TreatmentSystemUsed,
                    TreatmentSystemType = entry.TreatmentSystemType,
                    TreatmentSuccessful = entry.TreatmentSuccessful,
                    TreatmentDetails = entry.TreatmentDetails,
                    ExceptionalCircumstances = entry.ExceptionalCircumstances,
                    SalinityBeforeExchange = entry.SalinityBeforeExchange,
                    SalinityAfterExchange = entry.SalinityAfterExchange,
                    PortName = entry.PortName,
                    ReceptionFacility = entry.ReceptionFacility,
                    ReceiptNumber = entry.ReceiptNumber,
                    OfficerInCharge = entry.OfficerInCharge,
                    MasterSignature = entry.MasterSignature,
                    SignedAt = entry.SignedAt,
                    Remarks = entry.Remarks,
                    IsSynced = entry.IsSynced,
                    CreatedAt = entry.CreatedAt,
                    UpdatedAt = entry.UpdatedAt,
                    OriginNode = entry.OriginNode
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving ballast water record entry {Id}", id);
                throw;
            }
        }

        public async Task<(bool Success, string? Error)> SignEntryAsync(Guid id, SignBallastWaterDto dto, string? username = null)
        {
            try
            {
                var entry = await _context.BallastWaterRecordBooks.FindAsync(id);
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
                _logger.LogError(ex, "Error signing ballast water record entry {Id}", id);
                return (false, "An error occurred while signing the entry.");
            }
        }

        public async Task<(bool Success, string? Error)> UpdateEntryAsync(Guid id, UpdateBallastWaterEntryDto dto, string? username = null)
        {
            try
            {
                var entry = await _context.BallastWaterRecordBooks.FindAsync(id);
                if (entry == null || entry.IsDeleted)
                {
                    return (false, "Entry not found.");
                }

                if (!string.IsNullOrEmpty(entry.MasterSignature))
                {
                    return (false, "Cannot update a signed entry.");
                }

                entry.OperationDateTime = dto.OperationDateTime;
                entry.OperationCode = dto.OperationCode;
                entry.OperationDescription = dto.OperationDescription;
                entry.BallastTank = dto.BallastTank;
                entry.Volume = dto.Volume;
                entry.StartLatitude = dto.StartLatitude;
                entry.StartLongitude = dto.StartLongitude;
                entry.StartDateTime = dto.StartDateTime;
                entry.EndLatitude = dto.EndLatitude;
                entry.EndLongitude = dto.EndLongitude;
                entry.EndDateTime = dto.EndDateTime;
                entry.WaterDepth = dto.WaterDepth;
                entry.DistanceFromLand = dto.DistanceFromLand;
                entry.ExchangeVolumePercentage = dto.ExchangeVolumePercentage;
                entry.ExchangeMethod = dto.ExchangeMethod;
                entry.TreatmentSystemUsed = dto.TreatmentSystemUsed;
                entry.TreatmentSystemType = dto.TreatmentSystemType;
                entry.TreatmentSuccessful = dto.TreatmentSuccessful;
                entry.TreatmentDetails = dto.TreatmentDetails;
                entry.ExceptionalCircumstances = dto.ExceptionalCircumstances;
                entry.SalinityBeforeExchange = dto.SalinityBeforeExchange;
                entry.SalinityAfterExchange = dto.SalinityAfterExchange;
                entry.PortName = dto.PortName;
                entry.ReceptionFacility = dto.ReceptionFacility;
                entry.ReceiptNumber = dto.ReceiptNumber;
                entry.OfficerInCharge = dto.OfficerInCharge;
                entry.Remarks = dto.Remarks;
                
                entry.UpdatedAt = DateTime.UtcNow;
                entry.IsSynced = false;

                await _context.SaveChangesAsync();
                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating ballast water record entry {Id}", id);
                return (false, "An error occurred while updating the entry.");
            }
        }
    }
}
