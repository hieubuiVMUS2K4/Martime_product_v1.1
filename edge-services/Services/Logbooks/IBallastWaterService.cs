using MaritimeEdge.DTOs.Logbooks;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MaritimeEdge.Services.Logbooks
{
    public interface IBallastWaterService
    {
        Task<(bool Success, Guid? Id, string? Error)> CreateEntryAsync(CreateBallastWaterEntryDto dto, string? username = null);
        Task<BallastWaterEntryResponseDto?> GetEntryAsync(Guid id);
        Task<PaginatedLogbookResponseDto<BallastWaterEntryResponseDto>> GetEntriesAsync(LogbookPaginationDto pagination);
        Task<(bool Success, string? Error)> UpdateEntryAsync(Guid id, UpdateBallastWaterEntryDto dto, string? username = null);
        Task<(bool Success, string? Error)> DeleteEntryAsync(Guid id, string? username = null);
        Task<(bool Success, string? Error)> SignEntryAsync(Guid id, SignBallastWaterDto dto, string? username = null);
    }
}
