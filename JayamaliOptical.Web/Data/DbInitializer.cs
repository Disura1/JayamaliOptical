using JayamaliOptical.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace JayamaliOptical.Web.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration)
        {
            // 1. Ensure Admin role exists
            if (!await roleManager.RoleExistsAsync("Admin"))
                await roleManager.CreateAsync(new IdentityRole("Admin"));

            if (!await roleManager.RoleExistsAsync("Customer"))
                await roleManager.CreateAsync(new IdentityRole("Customer"));

            // 2. Seed admin user only if not already present
            var adminEmail = "jayamalioptical@gmail.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                var adminPassword = configuration["AdminSettings:DefaultPassword"];

                if (string.IsNullOrEmpty(adminPassword))
                {
                    // Log warning but don't crash — admin can be created manually
                    Console.WriteLine("WARNING: AdminSettings:DefaultPassword is not set. Admin user not created.");
                }
                else
                {
                    adminUser = new IdentityUser
                    {
                        UserName = adminEmail,
                        Email = adminEmail,
                        EmailConfirmed = true
                    };

                    var result = await userManager.CreateAsync(adminUser, adminPassword);
                    if (result.Succeeded)
                        await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // 3. Seed categories
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

            // 4. Seed services
            if (!context.Services.Any())
            {
                context.Services.AddRange(
                    new Service
                    {
                        Name = "Eye Channeling",
                        Description = "Consultation with specialist eye doctors",
                        Price = 1500,
                        DurationMinutes = 30,
                        RequiresAppointment = true,
                        Button1Text = "Book Appointment",
                        Button1Url = "/Appointments/Create",
                        Button1Class = "btn-success",
                        Button2Text = "View Doctors",
                        Button2Url = "/Doctors",
                        Button2Class = "btn-info",
                        DisplayOrder = 1,
                        IsActive = true
                    },
                    new Service
                    {
                        Name = "Orthoptic Assessment",
                        Description = "Comprehensive eye movement and binocular vision assessment",
                        Price = 2500,
                        DurationMinutes = 45,
                        RequiresAppointment = true,
                        Button1Text = "Book Assessment",
                        Button1Url = "/Appointments/Create",
                        Button1Class = "btn-success",
                        Button2Text = "Contact Specialist",
                        Button2Url = "/Contact",
                        Button2Class = "btn-info",
                        DisplayOrder = 8,
                        IsActive = true
                    }
                );
                await context.SaveChangesAsync();
            }
        }
    }
}