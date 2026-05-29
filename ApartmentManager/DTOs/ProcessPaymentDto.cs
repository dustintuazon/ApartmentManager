using ApartmentManager.Models;

namespace ApartmentManager.DTOs
{
    public class ProcessPaymentDto
    {
        public int TenantId { get; set; }
        public int Amount { get; set; }
        public Purpose Purpose { get; set; }
        public ModeOfPayment MOP { get; set; }
        public DateOnly DatePaid { get; set; }
        public string? ReferenceNumber { get; set; }
        public string UserId { get; set; } = string.Empty;
    }
}
