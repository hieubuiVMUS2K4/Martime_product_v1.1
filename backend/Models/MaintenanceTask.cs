namespace ProductApi.Models
{
    public class MaintenanceTask
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = string.Empty;
        public DateTime ScheduledAt { get; set; }
        public Guid ShipId { get; set; }
    }
}
