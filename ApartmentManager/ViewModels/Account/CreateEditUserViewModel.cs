using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace ApartmentManager.ViewModels.Account
{
    public class CreateEditUserViewModel
    {
        public string? Id { get; set; } = string.Empty;

        [Required]
        public string Username { get; set; } = string.Empty;
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        [Required]
        [DataType(DataType.Password)]
        [Compare("ConfirmPassword", ErrorMessage = "Password do not match")]
        public string Password { get; set; } = string.Empty;
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; } = string.Empty;
        [Required]
        public string Role { get; set; } = string.Empty;
        public IEnumerable<SelectListItem>? Roles { get; set; }
        public bool IsEditing { get; set; } = false;

    }
}
