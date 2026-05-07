namespace TenPercent.Data
{
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using TenPercent.Data.Models; // Задължително за да вижда User класа

    public static class DbSeeder
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
        {
            // Променяме типовете тук!
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<User>>();

            // Роли
            string[] roles = { "Admin", "Player" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole<int>(role));
                }
            }

            // Admin user
            string email = "admin@tenpercent.com";
            string password = "123";

            var user = await userManager.FindByEmailAsync(email);

            if (user == null)
            {
                // Използваме ТВОЯ модел User
                user = new User
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    Role = "Admin", // Задаваме и твоето custom поле
                    CreatedAt = DateTime.UtcNow // Задаваме и твоето custom поле
                };

                var result = await userManager.CreateAsync(user, password);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Admin");
                }
                else
                {
                    throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
        }
    }
}