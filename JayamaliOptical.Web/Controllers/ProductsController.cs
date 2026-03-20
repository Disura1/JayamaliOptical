using JayamaliOptical.Web.Data;
using JayamaliOptical.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JayamaliOptical.Web.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Products
        public async Task<IActionResult> Index(int? categoryId)
        {
            var products = _context.Products.Include(p => p.Category).AsQueryable();

            if (categoryId.HasValue)
            {
                products = products.Where(p => p.CategoryId == categoryId.Value);
            }

            ViewBag.Categories = await _context.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();

            ViewBag.SelectedCategoryId = categoryId;

            return View(await products.OrderBy(p => p.Name).ToListAsync());
        }

        // GET: Products/Details/5 (Returns JSON for modal)
        [HttpGet]
        public async Task<IActionResult> GetProductDetails(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            return Json(new
            {
                id = product.Id,
                name = product.Name,
                description = product.Description,
                price = product.Price,
                brand = product.Brand,
                stockQuantity = product.StockQuantity,
                category = product.Category?.Name,
                frameType = product.FrameType,
                lensPower = product.LensPower,
                iolType = product.IOLType,
                productType = product.ProductType,
                imageUrl1 = product.ImageUrl1,
                imageUrl2 = product.ImageUrl2,
                isActive = product.IsActive,
                requiresPrescription = product.RequiresPrescription
            });
        }
    }
}