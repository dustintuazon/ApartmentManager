using ApartmentManager.Models;
using ApartmentManager.ViewModels.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var accounts = await userManager.Users.Include(r=>r.Rooms).ToListAsync();
            
            List<ViewUserViewModel> accountsViewModel = new List<ViewUserViewModel>();

            foreach(var account in accounts)
            {
                var roles = await userManager.GetRolesAsync(account);
                var roomCount = account.Rooms?.Count ?? 0;

                var accountViewModel = new ViewUserViewModel
                {
                    Id = account.Id,
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

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> CreateEditUser(string? id)
        {
            var roleList = roleManager.Roles.ToList().Select(r => new SelectListItem
            {
                Value = r.Id,
                Text = r.Name
            });

            var userModel = new CreateEditUserViewModel() 
            {
                Roles = roleList
            };

            //edit user
            if (id is not null)
            {
                var user = await userManager.FindByIdAsync(id);
                var userRoles = await userManager.GetRolesAsync(user);
                var selectedRole = roleManager.Roles.ToList().FirstOrDefault(r=> userRoles.Contains(r.Name));

                userModel.Id = user.Id;
                userModel.Username = user.UserName;
                userModel.Email = user.Email;
                userModel.Role = selectedRole.Id;
                userModel.IsEditing = true;

                return View(userModel);
            }

            //create user
            return View(userModel);
        }

        [Authorize(Roles ="Admin")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> CreateEditUser(CreateEditUserViewModel model)
        {
            var roleList = roleManager.Roles.ToList().Select(r => new SelectListItem
            {
                Value = r.Id,
                Text = r.Name
            });

            model.Roles = roleList;

            //edit
            if (model.IsEditing)
            {
                var user = await userManager.FindByIdAsync(model.Id);

                user.UserName = model.Username;
                user.NormalizedUserName = model.Username.ToUpper();
                user.Email = model.Email;
                user.NormalizedEmail = model.Email.ToUpper();

                var result = await userManager.UpdateAsync(user);
                if(result.Succeeded)
                {
                    var currentRole = await userManager.GetRolesAsync(user);
                    var selectedRole = await roleManager.FindByIdAsync(model.Role);
                    if (!currentRole.Contains(selectedRole.Name))
                    {
                        await userManager.RemoveFromRolesAsync(user, currentRole);
                        await userManager.AddToRoleAsync(user, selectedRole.Name);
                        await signInManager.RefreshSignInAsync(user);
                    }

                    return RedirectToAction("Index", "Account");
                }
                ModelState.AddModelError("", string.Join("\n", result.Errors.Select(e => e.Description)));
                model.IsEditing = true;
                return View(model);
            }
            else // create
            {
                var user = new User()
                {
                    UserName = model.Username,
                    NormalizedUserName = model.Username.ToUpper(),
                    Email = model.Email,
                    NormalizedEmail = model.Email.ToUpper(),
                };

                var result = await userManager.CreateAsync(user, model.Password);

                if(result.Succeeded)
                {
                    var role = await roleManager.FindByIdAsync(model.Role);
                    await userManager.AddToRoleAsync(user, role.Name);
                    return RedirectToAction("Index", "Account");
                }
                ModelState.AddModelError("", string.Join("\n", result.Errors.Select(e => e.Description)));
                model.IsEditing = false;
                return View(model);
            }
        }
    }
}
