using Microsoft.AspNetCore.Identity;

namespace ApartmentManager.Models
{
    public class User : IdentityUser
    {
        public ICollection<Room>? Rooms { get; set; }
        public ICollection<Transaction>? Transactions { get; set; }
        public ICollection<Tenant>? Tenants { get; set; }
    }
}
