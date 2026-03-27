using JayamaliOptical.Web.Data;
using JayamaliOptical.Web.Models;
using JayamaliOptical.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JayamaliOptical.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class BrandsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public BrandsController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public async Task<IActionResult> Index()
        {
            var brands = await _context.Brands.OrderBy(b => b.DisplayOrder).ToListAsync();
            return View(brands);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(string name, IFormFile? logoFile, int displayOrder)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                TempData["Error"] = "Brand name is required.";
                return RedirectToAction(nameof(Index));
            }

            string? logoPath = null;

            if (logoFile != null && logoFile.Length > 0)
            {
                // --- LAYER 1: EXTENSION VALIDATION ---
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".svg" };
                var ext = Path.GetExtension(logoFile.FileName).ToLower();

                // --- LAYER 2: CONTENT TYPE (MIME) VALIDATION ---
                // We add "image/svg+xml" for SVG support
                var allowedMimeTypes = new[] { "image/jpeg", "image/png", "image/webp", "image/svg+xml" };
                var contentType = logoFile.ContentType.ToLower();

                if (!allowedExtensions.Contains(ext) || !allowedMimeTypes.Contains(contentType))
                {
                    TempData["Error"] = "Invalid file type. Only real JPG, PNG, WebP, and SVG images are allowed.";
                    return RedirectToAction(nameof(Index));
                }

                // --- LAYER 3: SIZE VALIDATION (Safety Bonus) ---
                if (logoFile.Length > 2 * 1024 * 1024) // Limit to 2MB
                {
                    TempData["Error"] = "File size must be less than 2MB.";
                    return RedirectToAction(nameof(Index));
                }

                var folder = Path.Combine(_environment.WebRootPath, "images", "Brands");
                Directory.CreateDirectory(folder);

                var fileName = Guid.NewGuid().ToString() + ext;
                var filePath = Path.Combine(folder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await logoFile.CopyToAsync(stream);
                }

                logoPath = "/images/Brands/" + fileName;
            }

            var brand = new Brand
            {
                Name = name.Trim(),
                LogoPath = logoPath,
                DisplayOrder = displayOrder,
                IsActive = true,
                CreatedDate = TimeHelper.Now
            };

            _context.Brands.Add(brand);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Brand '{name}' added successfully!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var brand = await _context.Brands.FindAsync(id);
            if (brand != null)
            {
                brand.IsActive = !brand.IsActive;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateOrder(int id, int order)
        {
            var brand = await _context.Brands.FindAsync(id);
            if (brand != null)
            {
                brand.DisplayOrder = order;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var brand = await _context.Brands.FindAsync(id);
            if (brand != null)
            {
                if (!string.IsNullOrEmpty(brand.LogoPath))
                {
                    var full = Path.Combine(_environment.WebRootPath,
                        brand.LogoPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                    if (System.IO.File.Exists(full)) System.IO.File.Delete(full);
                }
                _context.Brands.Remove(brand);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Brand deleted.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}