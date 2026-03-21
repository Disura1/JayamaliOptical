using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using JayamaliOptical.Web.Models;

namespace JayamaliOptical.Web.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Add these DbSets
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<JayamaliOptical.Web.Models.Service> Services { get; set; }
        public DbSet<JayamaliOptical.Web.Models.Order> Orders { get; set; }
        public DbSet<JayamaliOptical.Web.Models.OrderItem> OrderItems { get; set; }
        public DbSet<JayamaliOptical.Web.Models.ServiceBooking> ServiceBookings { get; set; }
        public DbSet<JayamaliOptical.Web.Models.UserProfile> UserProfiles { get; set; }
        public DbSet<JayamaliOptical.Web.Models.Prescription> Prescriptions { get; set; }
        public DbSet<JayamaliOptical.Web.Models.SiteSettings> SiteSettings { get; set; }
        public DbSet<JayamaliOptical.Web.Models.ContactMessage> ContactMessages { get; set; }
        public DbSet<JayamaliOptical.Web.Models.OfferSlide> OfferSlides { get; set; }
        public DbSet<JayamaliOptical.Web.Models.Brand> Brands { get; set; }
    }
}