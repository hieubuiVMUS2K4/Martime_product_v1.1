using MaritimeEdge.DTOs.Logbooks;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MaritimeEdge.Services.Logbooks
{
    public interface IDeckLogbookService
    {
        Task<(bool Success, Guid? Id, string? Error)> CreateEntryAsync(CreateDeckLogEntryDto dto, string? username = null);
        Task<DeckLogEntryResponseDto?> GetEntryAsync(Guid id);
        Task<PaginatedLogbookResponseDto<DeckLogEntryResponseDto>> GetEntriesAsync(LogbookPaginationDto pagination);
        Task<(bool Success, string? Error)> UpdateEntryAsync(Guid id, UpdateDeckLogEntryDto dto, string? username = null);
        Task<(bool Success, string? Error)> DeleteEntryAsync(Guid id, string? username = null); // Soft delete
        Task<(bool Success, string? Error)> SignEntryAsync(Guid id, SignDeckLogDto dto, string? username = null);
    }
}
