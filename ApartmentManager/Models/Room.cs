namespace ApartmentManager.Models
{
    public class Room
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Monthly { get; set; }
        public bool IsAvailable { get; set; } = true;
        public int Deposit { get; set; }
        public int Advance { get; set; }
        public Tenant? Tenant { get; set; }
        public string UserId { get; set; } = string.Empty;
        public User? User { get; set; }
    }
}

//TODO: Add type of room ex. Studio, One Bedroom, Two Bedroom, etc.
//TODO: Add size of room ex. 20 sqm, 30 sqm, etc.
