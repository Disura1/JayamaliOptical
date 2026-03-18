using JayamaliOptical.Web.Models;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using System.Linq;

namespace JayamaliOptical.Web.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            // Create Admin role if not exists
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            // Create admin user if not exists
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
                    // Assign Admin role
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // Create sample user role (optional)
            if (!await roleManager.RoleExistsAsync("Customer"))
            {
                await roleManager.CreateAsync(new IdentityRole("Customer"));
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

            // Create default optical services
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
                        // Icon removed - use ImageUrl for images
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
                        Name = "HVF Testing",
                        Description = "Humphrey Visual Field testing for glaucoma detection",
                        Price = 2500,
                        DurationMinutes = 45,
                        RequiresAppointment = true,
                        Button1Text = "Book Test",
                        Button1Url = "/Appointments/Create",
                        Button1Class = "btn-success",
                        Button2Text = "Learn More",
                        Button2Url = "/Services/HVF",
                        Button2Class = "btn-info",
                        DisplayOrder = 2,
                        IsActive = true
                    },
                    new Service
                    {
                        Name = "OCT Testing",
                        Description = "Optical Coherence Tomography for retina scanning",
                        Price = 3500,
                        DurationMinutes = 30,
                        RequiresAppointment = true,
                        Button1Text = "Book Test",
                        Button1Url = "/Appointments/Create",
                        Button1Class = "btn-success",
                        Button2Text = "Contact Clinic",
                        Button2Url = "/Contact",
                        Button2Class = "btn-info",
                        DisplayOrder = 3,
                        IsActive = true
                    },
                    new Service
                    {
                        Name = "Cataract Surgeries",
                        Description = "Advanced cataract surgery with IOL implantation",
                        Price = 85000,
                        DurationMinutes = 120,
                        RequiresAppointment = true,
                        Button1Text = "Consult Surgeon",
                        Button1Url = "/Appointments/Create",
                        Button1Class = "btn-danger",
                        Button2Text = "Learn About Surgery",
                        Button2Url = "/Services/Cataract",
                        Button2Class = "btn-info",
                        Button3Text = "Insurance Info",
                        Button3Url = "/Insurance",
                        Button3Class = "btn-warning",
                        DisplayOrder = 4,
                        IsActive = true
                    },
                    new Service
                    {
                        Name = "Hess Chart",
                        Description = "Eye muscle function testing for squint diagnosis",
                        Price = 2000,
                        DurationMinutes = 30,
                        RequiresAppointment = true,
                        Button1Text = "Book Test",
                        Button1Url = "/Appointments/Create",
                        Button1Class = "btn-success",
                        DisplayOrder = 5,
                        IsActive = true
                    },
                    new Service
                    {
                        Name = "Colour Vision",
                        Description = "Colour blindness testing and assessment",
                        Price = 1000,
                        DurationMinutes = 15,
                        RequiresAppointment = false,
                        Button1Text = "Walk-in Available",
                        Button1Url = "/Services/ColourVision",
                        Button1Class = "btn-info",
                        DisplayOrder = 6,
                        IsActive = true
                    },
                    new Service
                    {
                        Name = "Laser Treatments",
                        Description = "LASIK and other laser vision correction procedures",
                        Price = 120000,
                        DurationMinutes = 60,
                        RequiresAppointment = true,
                        Button1Text = "Free Consultation",
                        Button1Url = "/Appointments/Create",
                        Button1Class = "btn-success",
                        Button2Text = "Eligibility Check",
                        Button2Url = "/Services/Laser/Eligibility",
                        Button2Class = "btn-info",
                        Button3Text = "Pricing Details",
                        Button3Url = "/Services/Laser/Pricing",
                        Button3Class = "btn-warning",
                        DisplayOrder = 7,
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