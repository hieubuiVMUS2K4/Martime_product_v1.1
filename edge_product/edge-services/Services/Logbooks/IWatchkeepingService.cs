using MaritimeEdge.DTOs.Logbooks;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MaritimeEdge.Services.Logbooks
{
    public interface IWatchkeepingService
    {
        Task<(bool Success, Guid? Id, string? Error)> CreateEntryAsync(CreateWatchkeepingLogDto dto, string? username = null);
        Task<WatchkeepingLogResponseDto?> GetEntryAsync(Guid id);
        Task<PaginatedLogbookResponseDto<WatchkeepingLogResponseDto>> GetEntriesAsync(LogbookPaginationDto pagination);
        Task<(bool Success, string? Error)> UpdateEntryAsync(Guid id, UpdateWatchkeepingLogDto dto, string? username = null);
        Task<(bool Success, string? Error)> DeleteEntryAsync(Guid id, string? username = null);
        Task<(bool Success, string? Error)> SignEntryAsync(Guid id, SignWatchkeepingLogDto dto, string? username = null);
    }
}
