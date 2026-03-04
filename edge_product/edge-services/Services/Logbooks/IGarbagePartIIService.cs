using MaritimeEdge.DTOs.Logbooks;
using System;
using System.Threading.Tasks;

namespace MaritimeEdge.Services.Logbooks
{
    public interface IGarbagePartIIService
    {
        Task<(bool Success, Guid? Id, string? Error)> CreateEntryAsync(CreateGarbagePartIIDto dto, string? username = null);
        Task<GarbagePartIIResponseDto?> GetEntryAsync(Guid id);
        Task<PaginatedLogbookResponseDto<GarbagePartIIResponseDto>> GetEntriesAsync(LogbookPaginationDto pagination);
        Task<(bool Success, string? Error)> UpdateEntryAsync(Guid id, UpdateGarbagePartIIDto dto, string? username = null);
        Task<(bool Success, string? Error)> DeleteEntryAsync(Guid id, string? username = null);
        Task<(bool Success, string? Error)> SignEntryAsync(Guid id, SignGarbagePartIIDto dto, string? username = null);
    }
}
