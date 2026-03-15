using JayamaliOptical.Web.Models;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using System.Linq;

namespace JayamaliOptical.Web.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(ApplicationDbContext context, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            // Create Admin Role if not exists
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            // Create Admin User if not exists
            var adminEmail = "admin@jayamalioptical.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new IdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, "Admin@123");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // Create default optical categories
            if (!context.Categories.Any())
            {
                context.Categories.AddRange(
                    new Category { Name = "Spectacle", Description = "Prescription eyeglasses", IsActive = true },
                    new Category { Name = "Sunglasses", Description = "UV protection sunglasses", IsActive = true },
                    new Category { Name = "Contact Lens", Description = "Soft and hard contact lenses", IsActive = true },
                    new Category { Name = "IOL Lens", Description = "Intraocular lenses for cataract surgery", IsActive = true }
                );
                await context.SaveChangesAsync();
            }
        }
    }
}