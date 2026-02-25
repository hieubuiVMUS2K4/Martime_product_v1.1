using MaritimeEdge.DTOs.Logbooks;
using System;
using System.Threading.Tasks;

namespace MaritimeEdge.Services.Logbooks
{
    public interface IGarbagePartIService
    {
        Task<(bool Success, Guid? Id, string? Error)> CreateEntryAsync(CreateGarbagePartIDto dto, string? username = null);
        Task<GarbagePartIResponseDto?> GetEntryAsync(Guid id);
        Task<PaginatedLogbookResponseDto<GarbagePartIResponseDto>> GetEntriesAsync(LogbookPaginationDto pagination);
        Task<(bool Success, string? Error)> UpdateEntryAsync(Guid id, UpdateGarbagePartIDto dto, string? username = null);
        Task<(bool Success, string? Error)> DeleteEntryAsync(Guid id, string? username = null);
        Task<(bool Success, string? Error)> SignEntryAsync(Guid id, SignGarbagePartIDto dto, string? username = null);
    }
}
