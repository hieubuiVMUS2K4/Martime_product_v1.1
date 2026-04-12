using ProductApi.DTOs;

namespace ProductApi.Services.AI.Conversation
{
    /// <summary>
    /// Service to manage conversation history and context
    /// </summary>
    public interface IConversationHistoryService
    {
        Task SaveMessageAsync(string sessionId, ConversationMessage message);
        Task<List<ConversationMessage>> GetHistoryAsync(string sessionId, int maxMessages = 10);
        Task ClearSessionAsync(string sessionId);
        Task<string> BuildContextFromHistoryAsync(string sessionId);
    }

    public class ConversationHistoryService : IConversationHistoryService
    {
        private readonly ILogger<ConversationHistoryService> _logger;
        private readonly Dictionary<string, List<ConversationMessage>> _sessions;
        private readonly int _maxSessionMessages = 20;

        public ConversationHistoryService(ILogger<ConversationHistoryService> logger)
        {
            _logger = logger;
            _sessions = new Dictionary<string, List<ConversationMessage>>();
        }

        /// <summary>
        /// Save a message to conversation history
        /// </summary>
        public async Task SaveMessageAsync(string sessionId, ConversationMessage message)
        {
            if (!_sessions.ContainsKey(sessionId))
            {
                _sessions[sessionId] = new List<ConversationMessage>();
            }

            _sessions[sessionId].Add(message);

            // Keep only recent messages to avoid excessive memory usage
            if (_sessions[sessionId].Count > _maxSessionMessages)
            {
                _sessions[sessionId] = _sessions[sessionId]
                    .Skip(_sessions[sessionId].Count - _maxSessionMessages)
                    .ToList();
            }

            _logger.LogDebug($"Saved message to session {sessionId}. Total messages: {_sessions[sessionId].Count}");

            await Task.CompletedTask;
        }

        /// <summary>
        /// Get conversation history
        /// </summary>
        public async Task<List<ConversationMessage>> GetHistoryAsync(string sessionId, int maxMessages = 10)
        {
            if (!_sessions.ContainsKey(sessionId))
            {
                return new List<ConversationMessage>();
            }

            var messages = _sessions[sessionId]
                .TakeLast(maxMessages)
                .ToList();

            _logger.LogDebug($"Retrieved {messages.Count} messages from session {sessionId}");

            return await Task.FromResult(messages);
        }

        /// <summary>
        /// Build context string from conversation history
        /// </summary>
        public async Task<string> BuildContextFromHistoryAsync(string sessionId)
        {
            var history = await GetHistoryAsync(sessionId, 5);

            if (history.Count == 0)
                return "No previous user context.";

            var contextLines = new System.Text.StringBuilder();
            contextLines.AppendLine("RECENT USER INTENTS:");

            foreach (var msg in history
                .Where(m => string.Equals(m.Role, "user", StringComparison.OrdinalIgnoreCase))
                .OrderBy(m => m.Timestamp))
            {
                var preview = msg.Content.Length > 100 ? msg.Content[..100] + "..." : msg.Content;
                contextLines.AppendLine($"  - {preview}");
            }

            if (contextLines.ToString().Trim() == "RECENT USER INTENTS:")
            {
                contextLines.AppendLine("  - (no recent user message)");
            }

            return await Task.FromResult(contextLines.ToString());
        }

        /// <summary>
        /// Clear conversation history for a session
        /// </summary>
        public async Task ClearSessionAsync(string sessionId)
        {
            if (_sessions.Remove(sessionId))
            {
                _logger.LogInformation($"Cleared session {sessionId}");
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// Get statistics for a session
        /// </summary>
        public int GetSessionMessageCount(string sessionId)
        {
            return _sessions.ContainsKey(sessionId) ? _sessions[sessionId].Count : 0;
        }

        /// <summary>
        /// Get total active sessions
        /// </summary>
        public int GetActiveSessionCount()
        {
            return _sessions.Count;
        }
    }
}
