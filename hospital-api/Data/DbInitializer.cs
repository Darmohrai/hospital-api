using hospital_api.Models.Auth;
using Microsoft.AspNetCore.Identity;

namespace hospital_api.Data;

public static class DbInitializer
{
    public static async Task Initialize(ApplicationDbContext context, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        context.Database.EnsureCreated();

        string[] roleNames = { "Admin", "Operator", "Authorized", "Guest" };
        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        IdentityUser adminUser = null;
        if (await userManager.FindByNameAsync("admin") == null)
        {
            adminUser = new IdentityUser { UserName = "admin", Email = "admin@example.com", EmailConfirmed = true };
            await userManager.CreateAsync(adminUser, "Admin123!");
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
        else { adminUser = await userManager.FindByNameAsync("admin"); }

        IdentityUser operatorUser = null;
        if (await userManager.FindByNameAsync("operator") == null)
        {
            operatorUser = new IdentityUser { UserName = "operator", Email = "operator@example.com", EmailConfirmed = true };
            await userManager.CreateAsync(operatorUser, "Operator123!");
            await userManager.AddToRoleAsync(operatorUser, "Operator");
        }
         else { operatorUser = await userManager.FindByNameAsync("operator"); }

        IdentityUser authorizedUser = null;
        if (await userManager.FindByNameAsync("authorized") == null)
        {
            authorizedUser = new IdentityUser { UserName = "authorized", Email = "authorized@example.com", EmailConfirmed = true };
            await userManager.CreateAsync(authorizedUser, "Authorized123!");
            await userManager.AddToRoleAsync(authorizedUser, "Authorized");
        }
         else { authorizedUser = await userManager.FindByNameAsync("authorized"); }

        IdentityUser guestUser = null;
        if (await userManager.FindByNameAsync("guest") == null)
        {
            guestUser = new IdentityUser { UserName = "guest", Email = "guest@example.com", EmailConfirmed = true };
            await userManager.CreateAsync(guestUser, "Guest123!");
            await userManager.AddToRoleAsync(guestUser, "Guest");
            // Створюємо заявку для гостя
            if (!context.UpgradeRequests.Any(r => r.UserId == guestUser.Id))
            {
                 context.UpgradeRequests.Add(new UpgradeRequest { UserId = guestUser.Id, User = guestUser });
            }
        }
         else { guestUser = await userManager.FindByNameAsync("guest"); }

        if (context.Hospitals.Any())
        {
            return;
        }
        await context.SaveChangesAsync();
    }
}