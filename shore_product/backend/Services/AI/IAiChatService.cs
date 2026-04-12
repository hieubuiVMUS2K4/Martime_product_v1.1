using ProductApi.DTOs;

namespace ProductApi.Services.AI
{
    public interface IAiChatService
    {
        Task<AiChatResponse> ChatAsync(AiChatRequest request, CancellationToken token);
    }
}
