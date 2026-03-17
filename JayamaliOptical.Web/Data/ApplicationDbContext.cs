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
    }
}