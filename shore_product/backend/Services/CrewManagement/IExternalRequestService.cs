using Maritime.Shared.DTOs.CrewManagement;

namespace ProductApi.Services.CrewManagement;

public interface IExternalRequestService
{
    // External Requests
    Task<List<ExternalRequestDto>> GetRequestsAsync(Guid? vesselId = null, string? status = null);
    Task<ExternalRequestDto?> GetRequestAsync(Guid id);
    Task<ExternalRequestDto> CreateRequestAsync(CreateExternalRequestRequest request);
    Task<ExternalRequestDto> UpdateRequestAsync(Guid id, UpdateExternalRequestRequest request);
    Task<ExternalRequestDto> ChangeStatusAsync(Guid id, ChangeExternalRequestStatusRequest request);
    Task DeleteRequestAsync(Guid id);

    // Candidates
    Task<List<ExternalCandidateDto>> GetCandidatesAsync(Guid requestId);
    Task<ExternalCandidateDto> SubmitCandidateAsync(SubmitCandidateRequest request);
    Task<ExternalCandidateDto> ReviewCandidateAsync(Guid candidateId, ReviewCandidateRequest request);

    // Messages
    Task<List<ExternalRequestMessageDto>> GetMessagesAsync(Guid requestId);
    Task<ExternalRequestMessageDto> AddMessageAsync(Guid requestId, CreateExternalMessageRequest request);
}
