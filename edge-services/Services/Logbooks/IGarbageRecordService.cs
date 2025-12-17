using MaritimeEdge.DTOs.Logbooks;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MaritimeEdge.Services.Logbooks
{
    public interface IGarbageRecordService
    {
        Task<(bool Success, Guid? Id, string? Error)> CreateEntryAsync(CreateGarbageRecordEntryDto dto, string? username = null);
        Task<GarbageRecordEntryResponseDto?> GetEntryAsync(Guid id);
        Task<PaginatedLogbookResponseDto<GarbageRecordEntryResponseDto>> GetEntriesAsync(LogbookPaginationDto pagination);
        Task<(bool Success, string? Error)> UpdateEntryAsync(Guid id, UpdateGarbageRecordEntryDto dto, string? username = null);
        Task<(bool Success, string? Error)> DeleteEntryAsync(Guid id, string? username = null);
        Task<(bool Success, string? Error)> SignEntryAsync(Guid id, SignGarbageRecordDto dto, string? username = null);
    }
}
