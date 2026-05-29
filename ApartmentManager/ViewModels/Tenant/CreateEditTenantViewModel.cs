using System.ComponentModel.DataAnnotations;

namespace ApartmentManager.ViewModels.Tenant
{
    public class CreateEditTenantViewModel
    {
        public int? Id { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;
        [Required]
        public int DepositPaid { get; set; }
        [Required]
        public int AdvancePaid { get; set; }
        [Required]
        [Display(Name = "Move In Date")]
        [DataType(DataType.Date)]
        public DateOnly MoveInDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);
    }
}
