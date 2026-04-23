namespace ApartmentManager.Models
{
    public class Room
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Monthly { get; set; }
        public bool IsAvailable { get; set; }
        public int Deposit { get; set; }
        public int Advance { get; set; }
        public Tenant? Tenant { get; set; }
        public string UserId { get; set; } = string.Empty;
        public User? User { get; set; }
    }
}
