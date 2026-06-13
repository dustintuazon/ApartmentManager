using ApartmentManager.Models;

namespace ApartmentManager.ViewModels.Transaction
{
    public class TransactionsViewModel
    {
        public string TenantName { get; set; } = string.Empty;
        public Purpose Purpose { get; set; }
        public int Amount { get; set; }
        public ModeOfPayment MOP { get; set; }
        public DateOnly Date { get; set; }
        public string? ReferenceNumber { get; set; } = string.Empty;
    }
}
