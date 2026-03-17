using ProductApi.DTOs;

namespace ProductApi.Services.Voyage;

public interface IVoyageService
{
    Task<FleetDashboardDto> GetFleetDashboardAsync();
    Task<VoyageTimelineDto> GetVoyageTimelineAsync(Guid voyageId, string? source = null, int? limit = null);
    Task<VoyagePerformanceDto> GetVoyagePerformanceAsync(Guid voyageId);
    Task<VoyageReviewDto?> GetVoyageReviewAsync(Guid voyageId);
    Task<VoyageReviewDto> UpsertVoyageReviewAsync(Guid voyageId, CreateVoyageReviewRequest request);
    Task<Guid> CreateVoyageAsync(CreateVoyageRequest request);
    Task UpdateVoyageAsync(Guid voyageId, UpdateVoyageRequest request);
    Task DeleteVoyageAsync(Guid voyageId);
}
