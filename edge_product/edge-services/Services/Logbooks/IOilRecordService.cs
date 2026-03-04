using MaritimeEdge.DTOs.Logbooks;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MaritimeEdge.Services.Logbooks
{
    public interface IOilRecordService
    {
        Task<(bool Success, Guid? Id, string? Error)> CreateEntryAsync(CreateOilRecordEntryDto dto, string? username = null);
        Task<OilRecordEntryResponseDto?> GetEntryAsync(Guid id);
        Task<PaginatedLogbookResponseDto<OilRecordEntryResponseDto>> GetEntriesAsync(LogbookPaginationDto pagination);
        Task<(bool Success, string? Error)> UpdateEntryAsync(Guid id, UpdateOilRecordEntryDto dto, string? username = null);
        Task<(bool Success, string? Error)> DeleteEntryAsync(Guid id, string? username = null);
        Task<(bool Success, string? Error)> SignEntryAsync(Guid id, SignOilRecordDto dto, string? username = null);
    }
}
