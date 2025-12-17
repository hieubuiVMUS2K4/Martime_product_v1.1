using MaritimeEdge.DTOs.Logbooks;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MaritimeEdge.Services.Logbooks
{
    public interface IEngineLogbookService
    {
        Task<(bool Success, Guid? Id, string? Error)> CreateEntryAsync(CreateEngineLogEntryDto dto, string? username = null);
        Task<EngineLogEntryResponseDto?> GetEntryAsync(Guid id);
        Task<PaginatedLogbookResponseDto<EngineLogEntryResponseDto>> GetEntriesAsync(LogbookPaginationDto pagination);
        Task<(bool Success, string? Error)> UpdateEntryAsync(Guid id, UpdateEngineLogEntryDto dto, string? username = null);
        Task<(bool Success, string? Error)> DeleteEntryAsync(Guid id, string? username = null);
        Task<(bool Success, string? Error)> SignEntryAsync(Guid id, SignEngineLogDto dto, string? username = null);
    }
}
