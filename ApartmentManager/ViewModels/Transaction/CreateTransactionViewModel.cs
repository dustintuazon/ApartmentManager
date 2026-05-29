using ApartmentManager.Models;
using System.ComponentModel.DataAnnotations;

namespace ApartmentManager.ViewModels.Transaction
{
    public class CreateTransactionViewModel
    {
        public int TenantId { get; set; }
        [Required]
        public int Amount { get; set; }
        [Required]
        [DataType(DataType.Date)]
        public DateOnly PaymentDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);
        [Required]
        public Purpose Purpose { get; set; }
        [Required]
        [Display(Name = "Mode of Payment")]
        public ModeOfPayment ModeOfPayment { get; set; }
        public string? ReferenceNumber { get; set; } = string.Empty;
    }
}
