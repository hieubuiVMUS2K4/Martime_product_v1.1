namespace ProductApi.DTOs
{
    public class AiChatRequest
    {
        public string Question { get; set; } = string.Empty;
        public Guid? VesselId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        // V2 Enhancement - optional properties for advanced features
        public string SessionId { get; set; } = Guid.NewGuid().ToString();
        public List<ConversationMessage> ConversationHistory { get; set; } = new();
        public string DetailLevel { get; set; } = "Detailed";
        public string? FocusArea { get; set; }
    }

    public class ConversationMessage
    {
        public string Role { get; set; } = "user";
        public string Content { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    public class AiChatResponse
    {
        public string Answer { get; set; } = string.Empty;
        public string Sources { get; set; } = string.Empty;
        public bool Success { get; set; } = true;
        public string ErrorSource { get; set; } = "none";
        public int? RetryAfterSeconds { get; set; }

        // V2 Enhancement - optional metadata
        public double Confidence { get; set; } = 0.5;
        public string DetectedIntent { get; set; } = "general_query";
        public List<string> FollowUpQuestions { get; set; } = new();
        public ResponseMetrics Metrics { get; set; } = new();
        public bool IsCached { get; set; } = false;
    }

    public class GeminiChatResult
    {
        public bool Success { get; set; } = true;
        public string Content { get; set; } = string.Empty;
        public string ErrorSource { get; set; } = "none";
        public int? RetryAfterSeconds { get; set; }
    }

    public class ResponseMetrics
    {
        public long DatabaseQueryMs { get; set; } = 0;
        public long ContextBuildingMs { get; set; } = 0;
        public long ApiCallMs { get; set; } = 0;
        public long TotalResponseMs { get; set; } = 0;
        public int DataPointsAnalyzed { get; set; } = 0;
    }
}
