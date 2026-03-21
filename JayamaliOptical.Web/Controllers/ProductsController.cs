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

        public async Task<IActionResult> Index(int? categoryId, string? search)
        {
            var products = _context.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive)
                .AsQueryable();

            // Category filter
            if (categoryId.HasValue)
                products = products.Where(p => p.CategoryId == categoryId.Value);

            // Search filter
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                products = products.Where(p =>
                    p.Name.ToLower().Contains(s) ||
                    (p.Description != null && p.Description.ToLower().Contains(s)) ||
                    (p.Brand != null && p.Brand.ToLower().Contains(s)) ||
                    (p.Category != null && p.Category.Name.ToLower().Contains(s))
                );
            }

            ViewBag.Categories = await _context.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();

            ViewBag.SelectedCategoryId = categoryId;
            ViewBag.SearchTerm = search;

            return View(await products.OrderBy(p => p.Name).ToListAsync());
        }

        [HttpGet]
        public async Task<IActionResult> GetProductDetails(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return NotFound();

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