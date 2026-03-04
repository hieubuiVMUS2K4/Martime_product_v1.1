namespace ProductApi.Models
{
    public class Ship
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string IMO { get; set; } = string.Empty;
        public int Capacity { get; set; }
    }
}
