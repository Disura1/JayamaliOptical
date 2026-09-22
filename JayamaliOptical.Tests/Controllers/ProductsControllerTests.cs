using JayamaliOptical.Web.Areas.Admin.Controllers;
using JayamaliOptical.Web.Data;
using JayamaliOptical.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace JayamaliOptical.Tests.Controllers
{
    public class ProductsControllerTests
    {
        private static ApplicationDbContext MakeContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var context = new ApplicationDbContext(options);
            context.Categories.AddRange(
                new Category { Id = 1, Name = "Contact Lens" },
                new Category { Id = 2, Name = "Spectacle" },
                new Category { Id = 3, Name = "Sunglasses" },
                new Category { Id = 4, Name = "IOL Lens" });
            context.SaveChanges();

            return context;
        }

        [Theory]
        [InlineData(2, true)]  // Spectacle
        [InlineData(3, true)]  // Sunglasses
        [InlineData(1, false)] // Contact Lens
        [InlineData(4, false)] // IOL Lens
        public async Task RequiresProductTypeAsync_LooksUpByCategoryName_NotHardcodedId(int categoryId, bool expected)
        {
            using var context = MakeContext();
            var controller = new ProductsController(context);

            var result = await controller.RequiresProductTypeAsync(categoryId);

            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task RequiresProductTypeAsync_UnknownCategoryId_ReturnsFalse()
        {
            using var context = MakeContext();
            var controller = new ProductsController(context);

            var result = await controller.RequiresProductTypeAsync(999);

            Assert.False(result);
        }
    }
}
