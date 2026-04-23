using System.ComponentModel.DataAnnotations;

namespace ApartmentManager.ViewModels.Account
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Username Required")]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Keep me signed in")]
        public bool RememberMe { get; set; }
    }
}
