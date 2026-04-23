using ApartmentManager.Models;
using ApartmentManager.ViewModels.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApartmentManager.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<User> signInManager;
        private readonly UserManager<User> userManager;
        private readonly RoleManager<IdentityRole> roleManager;

        public AccountController(SignInManager<User> signInManager, UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
            this.roleManager = roleManager;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            var accounts = await userManager.Users.Include(r=>r.Rooms).ToListAsync();
            
            List<ViewAccountViewModel> accountsViewModel = new List<ViewAccountViewModel>();

            foreach(var account in accounts)
            {
                var roles = await userManager.GetRolesAsync(account);
                var roomCount = account.Rooms?.Count ?? 0;

                var accountViewModel = new ViewAccountViewModel
                {
                    Username = account.UserName,
                    Email = account.Email,
                    Roles = string.Join(",", roles.ToArray()),
                    NumberOfRooms = roomCount
                };
                accountsViewModel.Add(accountViewModel);
            }

            return View(accountsViewModel);
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await signInManager.PasswordSignInAsync(model.Username, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("","Invalid username or password");
            return View(model);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }
    }
}
