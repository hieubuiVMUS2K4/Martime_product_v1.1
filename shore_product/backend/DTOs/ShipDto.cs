namespace ProductApi.DTOs
{
    public class ShipDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string IMO { get; set; } = string.Empty;
        public int Capacity { get; set; }
    }
}
