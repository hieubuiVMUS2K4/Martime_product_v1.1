using Maritime.Shared.DTOs.CrewManagement;

namespace ProductApi.Services.CrewManagement;

public interface ITravelService
{
    // Travel Requests
    Task<List<TravelRequestDto>> GetRequestsAsync(Guid? assignmentId = null, Guid? crewMemberId = null, string? status = null);
    Task<TravelRequestDto?> GetRequestAsync(Guid id);
    Task<TravelRequestDto> CreateRequestAsync(CreateTravelRequestRequest request);
    Task<TravelRequestDto> UpdateRequestAsync(Guid id, UpdateTravelRequestRequest request);
    Task<TravelRequestDto> ChangeStatusAsync(Guid id, ChangeTravelStatusRequest request);
    Task DeleteRequestAsync(Guid id);

    // Auto-generation
    Task<TravelRequestDto?> AutoGenerateFromAssignmentAsync(Guid assignmentId);

    // Segments
    Task<TravelSegmentDto> AddSegmentAsync(CreateTravelSegmentRequest request);
    Task DeleteSegmentAsync(Guid segmentId);

    // Status History
    Task<List<TravelStatusHistoryDto>> GetStatusHistoryAsync(Guid travelRequestId);
}
