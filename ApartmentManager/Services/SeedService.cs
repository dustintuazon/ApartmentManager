using ApartmentManager.Data;
using ApartmentManager.Models;
using Microsoft.AspNetCore.Identity;

namespace ApartmentManager.Services
{
    public class SeedService
    {
        public static async Task SeedDatabase(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            List<string> roles = new() {"Admin", "Owner"};

            var userAdmin = new User
            {
                UserName = "admin",
                NormalizedUserName = "ADMIN",
                Email = "dustuazon@gmail.com",
                NormalizedEmail = "DUSTUAZON@GMAIL.COM",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString(),
            };

            try
            {
                await context.Database.EnsureCreatedAsync();

                foreach (var role in roles)
                {
                    if (!await roleManager.RoleExistsAsync(role))
                    {
                        var result = await roleManager.CreateAsync(new IdentityRole(role));
                        if (!result.Succeeded)
                        {
                            throw new Exception($"Failed to create the role '{role}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
                        }
                    }
                }

                if (await userManager.FindByEmailAsync(userAdmin.Email) == null)
                {
                    var result = await userManager.CreateAsync(userAdmin, "admin123");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(userAdmin, "Admin");
                    }
                    else
                    {
                        throw new Exception($"Failed to assign role to user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }
                }
            }
            catch
            {

            }
        }
    }
}
