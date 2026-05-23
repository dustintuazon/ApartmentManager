namespace ApartmentManager.Models
{
    public class Transaction
    {
        public int Id { get; set; }
        public int Amount { get; set; }
        public DateOnly Date { get; set; }
        public Purpose Purpose { get; set; }
        public int TenantId { get; set; }
        public Tenant? Tenant { get; set; }
        public ModeOfPayment ModeOfPayment { get; set; }
        public string? ReferenceNumber { get; set; } = string.Empty;
        public string? UserId { get; set; } = string.Empty;
        public User? User { get; set; }
    }

    public enum Purpose
    {
        Monthly = 0,
        Balance = 1,
        Deposit = 2
    }

    public enum ModeOfPayment
    {
        Gcash = 0,
        Cash = 1,
    }
}
