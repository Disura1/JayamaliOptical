using JayamaliOptical.Web.Data;
using JayamaliOptical.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JayamaliOptical.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class GalleryController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public GalleryController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public async Task<IActionResult> Index()
        {
            var images = await _context.GalleryImages
                .OrderBy(g => g.DisplayOrder)
                .ToListAsync();
            return View(images);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(IFormFile imageFile, string? caption,
            string? category, int displayOrder)
        {
            if (imageFile == null || imageFile.Length == 0)
            {
                TempData["Error"] = "Please select an image.";
                return RedirectToAction(nameof(Index));
            }

            var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var ext = Path.GetExtension(imageFile.FileName).ToLower();
            if (!allowed.Contains(ext))
            {
                TempData["Error"] = "Only JPG, PNG, WebP allowed.";
                return RedirectToAction(nameof(Index));
            }

            var allowedMimeTypes = new[] { "image/jpeg", "image/png", "image/webp" };
            if (!allowedMimeTypes.Contains(imageFile.ContentType.ToLower()))
            {
                TempData["Error"] = "Invalid file type.";
                return RedirectToAction(nameof(Index));
            }

            if (imageFile.Length > 8 * 1024 * 1024)
            {
                TempData["Error"] = "Image must be under 8MB.";
                return RedirectToAction(nameof(Index));
            }

            var folder = Path.Combine(_environment.WebRootPath, "images", "Gallery");
            Directory.CreateDirectory(folder);
            var fileName = Guid.NewGuid().ToString() + ext;
            var filePath = Path.Combine(folder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
                await imageFile.CopyToAsync(stream);

            _context.GalleryImages.Add(new GalleryImage
            {
                ImagePath = "/images/Gallery/" + fileName,
                Caption = caption?.Trim(),
                Category = category?.Trim(),
                DisplayOrder = displayOrder,
                IsActive = true,
                CreatedDate = DateTime.Now
            });
            await _context.SaveChangesAsync();

            TempData["Success"] = "Image uploaded successfully!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var img = await _context.GalleryImages.FindAsync(id);
            if (img != null) { img.IsActive = !img.IsActive; await _context.SaveChangesAsync(); }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateOrder(int id, int order)
        {
            var img = await _context.GalleryImages.FindAsync(id);
            if (img != null) { img.DisplayOrder = order; await _context.SaveChangesAsync(); }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var img = await _context.GalleryImages.FindAsync(id);
            if (img != null)
            {
                if (!string.IsNullOrEmpty(img.ImagePath))
                {
                    var full = Path.Combine(_environment.WebRootPath,
                        img.ImagePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                    if (System.IO.File.Exists(full)) System.IO.File.Delete(full);
                }
                _context.GalleryImages.Remove(img);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Image deleted.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}