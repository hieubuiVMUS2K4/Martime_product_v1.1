using Microsoft.AspNetCore.Mvc;
using MaritimeEdge.Services.AI;

namespace MaritimeEdge.Controllers.AI;

/// <summary>
/// AI Chat assistant for maritime crew — provides safety info, regulations, task guidance
/// </summary>
[ApiController]
[Route("api/chat")]
public class ChatController : ControllerBase
{
    private readonly IChatService _chatService;
    private readonly ILogger<ChatController> _logger;

    public ChatController(IChatService chatService, ILogger<ChatController> logger)
    {
        _chatService = chatService;
        _logger = logger;
    }

    /// <summary>
    /// Send a message to the AI assistant and get a response
    /// </summary>
    [HttpPost("send")]
    public async Task<IActionResult> SendMessage([FromBody] ChatRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
            return BadRequest(new { error = "Message cannot be empty" });

        // Limit message length to prevent abuse
        if (request.Message.Length > 2000)
            return BadRequest(new { error = "Message too long (max 2000 characters)" });

        try
        {
            var response = await _chatService.ProcessMessageAsync(request.Message, request.Context);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing chat message");
            return StatusCode(500, new { error = "Failed to process message. Please try again." });
        }
    }

    /// <summary>
    /// Get suggested questions for the chat interface
    /// </summary>
    [HttpGet("suggestions")]
    public IActionResult GetSuggestions()
    {
        var suggestions = _chatService.GetSuggestedQuestions();
        return Ok(suggestions);
    }
}
