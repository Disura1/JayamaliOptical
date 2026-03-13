using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace JayamaliOptical.Web.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Add your DbSet properties here for your models
        // Example:
        // public DbSet<Product> Products { get; set; }
        // public DbSet<Customer> Customers { get; set; }
    }
}