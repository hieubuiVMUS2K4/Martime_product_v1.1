namespace ProductApi.Models
{
    public class CrewMember
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = null!;
        public string Role { get; set; } = null!;
        public Guid ShipId { get; set; }
    }
}
