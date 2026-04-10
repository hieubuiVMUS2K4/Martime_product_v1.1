namespace ProductApi.DTOs
{
    public class ChatRequestDto
    {
        public string Message { get; set; } = string.Empty;
        public string VesselId { get; set; } = string.Empty; // Guid string
        public string? SessionId { get; set; }
    }
}
