using System.ComponentModel.DataAnnotations;

namespace ApartmentManager.ViewModels.Room
{
    public class CreateEditRoomViewModel
    {
        public int? Id { get; set; }
        [Required]
        [Display(Name = "Room Name/Number")]
        public string Name { get; set; } = string.Empty;
        [Required(ErrorMessage = "Description is required.")]
        [Display(Name = "Description ex. Studio, One Bedroom, Two Bedroom")]
        public string Description { get; set; } = string.Empty;
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Monthly rent must be a positive number.")]
        [Display(Name = "Monthly Rent")]
        public int Monthly { get; set; }
        [Required]
        [Display(Name = "Deposit (months)")]
        [Range(1, 12, ErrorMessage = "Deposit must be between 1 and 12 months.")]
        public int MonthsDeposit { get; set; }
        [Required]
        [Display(Name = "Advance (months)")]
        [Range(1, 12, ErrorMessage = "Advance must be between 1 and 12 months.")]
        public int MonthsAdvanced { get; set; }
    }
}
