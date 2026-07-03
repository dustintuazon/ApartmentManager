using System.ComponentModel.DataAnnotations;

namespace ApartmentManager.ViewModels.Tenant
{
    public class MoveOutDateViewModel
    {
        [Required]
        [Display(Name = "Move Out Date")]
        [DataType(DataType.Date)]
        public DateOnly MoveOutDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);
        public string ResultMessage { get; set; } = string.Empty;
    }
}
