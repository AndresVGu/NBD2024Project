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

                // Safety net: ensure role audit table exists even if migrations are not applied yet.
                await context.Database.ExecuteSqlRawAsync(
                    "CREATE TABLE IF NOT EXISTS \"RoleAuditLogs\" (\"ID\" INTEGER NOT NULL CONSTRAINT \"PK_RoleAuditLogs\" PRIMARY KEY AUTOINCREMENT, \"ActorUserId\" TEXT NULL, \"ActorUserName\" TEXT NULL, \"TargetUserId\" TEXT NULL, \"TargetUserName\" TEXT NULL, \"ActionType\" TEXT NOT NULL, \"RoleName\" TEXT NULL, \"Notes\" TEXT NULL, \"CreatedOnUtc\" TEXT NOT NULL);");
                await context.Database.ExecuteSqlRawAsync(
                    "CREATE INDEX IF NOT EXISTS \"IX_RoleAuditLogs_CreatedOnUtc\" ON \"RoleAuditLogs\" (\"CreatedOnUtc\");");
                await context.Database.ExecuteSqlRawAsync(
                    "CREATE INDEX IF NOT EXISTS \"IX_RoleAuditLogs_TargetUserId_CreatedOnUtc\" ON \"RoleAuditLogs\" (\"TargetUserId\", \"CreatedOnUtc\");");

                //Create Roles
                string[] roleNames = { "Root", "Admin", "Supervisor", "Sales", "Designer" };
 
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

                //ROOT (super admin default account)
                const string rootUserName = "root";
                const string rootEmail = "root@test.com";
                const string rootPassword = "R00t.%2109";

                var rootUser = await userManager.FindByNameAsync(rootUserName)
                    ?? await userManager.FindByEmailAsync(rootEmail);
                if (rootUser == null)
                {
                    rootUser = new IdentityUser
                    {
                        UserName = rootUserName,
                        Email = rootEmail,
                        EmailConfirmed = true
                    };

                    var rootCreate = await userManager.CreateAsync(rootUser, rootPassword);
                    if (!rootCreate.Succeeded)
                    {
                        return;
                    }
                }
                else
                {
                    if (!string.Equals(rootUser.UserName, rootUserName, StringComparison.OrdinalIgnoreCase)
                        || !string.Equals(rootUser.Email, rootEmail, StringComparison.OrdinalIgnoreCase)
                        || !rootUser.EmailConfirmed)
                    {
                        rootUser.UserName = rootUserName;
                        rootUser.Email = rootEmail;
                        rootUser.EmailConfirmed = true;
                        await userManager.UpdateAsync(rootUser);
                    }
                }

                // Root inherits all permissions by belonging to every role.
                var allRoles = await roleManager.Roles.Select(r => r.Name).ToListAsync();
                var rootRoles = await userManager.GetRolesAsync(rootUser);
                foreach (var role in allRoles)
                {
                    if (!rootRoles.Contains(role))
                    {
                        await userManager.AddToRoleAsync(rootUser, role);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.GetBaseException().Message);
            }
        }
    }
}
