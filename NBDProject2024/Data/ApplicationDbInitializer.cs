using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace NBDProject2024.Data
{
    public static class ApplicationDbInitializer
    {
        public static async Task SeedAsync(IApplicationBuilder applicationBuilder)
        {
            try
            {
                using var scope = applicationBuilder.ApplicationServices.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

                // Only apply migrations if there are pending changes.
                var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
                if (pendingMigrations.Any())
                {
                    await context.Database.MigrateAsync();
                }

                //Create Roles
                string[] roleNames = { "Admin", "Supervisor", "Sales", "Designer" };
 
                foreach (var roleName in roleNames)
                {
                    var roleExist = await roleManager.RoleExistsAsync(roleName);
                    if (!roleExist)
                    {
                        await roleManager.CreateAsync(new IdentityRole(roleName));
                    }
                }

                //Create Users:
                //ADMIN
                if (await userManager.FindByEmailAsync("admin@outlook.com") == null)
                {
                    IdentityUser user = new IdentityUser
                    {
                        UserName = "admin@outlook.com",
                        Email = "admin@outlook.com",
                        EmailConfirmed = true
                    };

                    IdentityResult result = await userManager.CreateAsync(user, "Pa55w@rd");

                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, "Admin");
                    }
                }
                //SUPERVISOR
                if (await userManager.FindByEmailAsync("super@outlook.com") == null)
                {
                    IdentityUser user = new IdentityUser
                    {
                        UserName = "super@outlook.com",
                        Email = "super@outlook.com",
                        EmailConfirmed = true
                    };

                    IdentityResult result = await userManager.CreateAsync(user, "Pa55w@rd");

                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, "Supervisor");
                    }
                }
                //SALES
                if (await userManager.FindByEmailAsync("sales@outlook.com") == null)
                {
                    IdentityUser user = new IdentityUser
                    {
                        UserName = "sales@outlook.com",
                        Email = "sales@outlook.com",
                        EmailConfirmed = true
                    };

                    IdentityResult result = await userManager.CreateAsync(user, "Pa55w@rd");

                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, "Sales");
                    }
                }
                //DESIGNER
                if (await userManager.FindByEmailAsync("designer@outlook.com") == null)
                {
                    IdentityUser user = new IdentityUser
                    {
                        UserName = "designer@outlook.com",
                        Email = "designer@outlook.com",
                        EmailConfirmed = true
                    };

                    IdentityResult result = await userManager.CreateAsync(user, "Pa55w@rd");

                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, "Designer");
                    }
                }
                //USER
                if (await userManager.FindByEmailAsync("user@outlook.com") == null)
                {
                    IdentityUser user = new IdentityUser
                    {
                        UserName = "user@outlook.com",
                        Email = "user@outlook.com",
                        EmailConfirmed = true
                    };

                    await userManager.CreateAsync(user, "Pa55w@rd");
                    //Not in any role
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.GetBaseException().Message);
            }
        }
    }
}
