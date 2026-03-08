using Maritime.Shared.DTOs.CrewManagement;

namespace ProductApi.Services.CrewManagement;

public interface IOnboardEventService
{
    // Onboard Events
    Task<List<OnboardEventDto>> GetEventsAsync(Guid? vesselId = null, Guid? crewMemberId = null, string? eventType = null);
    Task<OnboardEventDto?> GetEventAsync(Guid id);
    Task<OnboardEventDto> CreateEventAsync(CreateOnboardEventRequest request);

    // Access Grants
    Task<List<CrewAccessGrantDto>> GetAccessGrantsAsync(Guid? vesselId = null, Guid? crewMemberId = null, string? status = null);
    Task<CrewAccessGrantDto?> GetAccessGrantAsync(Guid id);
    Task<CrewAccessGrantDto> GrantAccessAsync(GrantAccessRequest request);
    Task<CrewAccessGrantDto?> SuspendAccessAsync(Guid id, SuspendAccessRequest request);
    Task<CrewAccessGrantDto?> RevokeAccessAsync(Guid id, RevokeAccessRequest request);
    Task<CrewAccessGrantDto?> ReinstateAccessAsync(Guid id, string? grantedBy);

    // Sign-On
    Task<List<SignOnRecordDto>> GetSignOnsAsync(Guid? vesselId = null, Guid? crewMemberId = null);
    Task<SignOnRecordDto?> GetSignOnAsync(Guid id);
    Task<SignOnRecordDto> CreateSignOnAsync(CreateSignOnRequest request);

    // Sign-Off
    Task<List<SignOffRecordDto>> GetSignOffsAsync(Guid? vesselId = null, Guid? crewMemberId = null);
    Task<SignOffRecordDto?> GetSignOffAsync(Guid id);
    Task<SignOffRecordDto> CreateSignOffAsync(CreateSignOffRequest request);
}
