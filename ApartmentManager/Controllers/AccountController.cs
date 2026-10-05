using ApartmentManager.Data;
using ApartmentManager.Models;
using ApartmentManager.ViewModels;
using ApartmentManager.ViewModels.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.EntityFrameworkCore;

namespace ApartmentManager.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<User> signInManager;
        private readonly UserManager<User> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly AppDbContext context;

        public AccountController(SignInManager<User> signInManager, UserManager<User> userManager, RoleManager<IdentityRole> roleManager, AppDbContext appDbContext)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
            this.roleManager = roleManager;
            context = appDbContext;
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index(string searchString, string currentSearch, int? pageNumber)
        {

            if(searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentSearch;
            }

            ViewData["CurrentSearch"] = searchString;

            var accounts = context.Users.Include(r=>r.Rooms).OrderBy(u => u.UserName).AsNoTracking();
            
            if(!string.IsNullOrEmpty(searchString))
            {
                accounts = accounts.Where(a => a.UserName.Contains(searchString) || a.Email.Contains(searchString));
            }

            var viewModel = accounts.Select(a => new ViewUserViewModel
            {
                Id = a.Id,
                Username = a.UserName,
                Email = a.Email,
                Roles = string.Join(",", context.UserRoles.Where(u => u.UserId == a.Id)
                    .Join(context.Roles,
                        userRole => userRole.RoleId,
                        role => role.Id,
                        (userRole, role) => role.Name)
                    .ToList()),
                NumberOfRooms = a.Rooms.Count
            });

            int pageSize = 10;

            return View(await PaginatedList<ViewUserViewModel>.CreateAsync(viewModel, pageNumber ?? 1, pageSize));
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
