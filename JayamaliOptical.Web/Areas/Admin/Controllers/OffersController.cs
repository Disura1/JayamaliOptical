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
    public class OffersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public OffersController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: Admin/Offers
        public async Task<IActionResult> Index()
        {
            var slides = await _context.OfferSlides
                .OrderBy(o => o.DisplayOrder)
                .ToListAsync();
            return View(slides);
        }

        // POST: Admin/Offers/Upload
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(IFormFile imageFile, string? caption, string? linkUrl, int displayOrder)
        {
            if (imageFile == null || imageFile.Length == 0)
            {
                TempData["Error"] = "Please select an image file.";
                return RedirectToAction(nameof(Index));
            }

            var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var ext = Path.GetExtension(imageFile.FileName).ToLower();
            if (!allowed.Contains(ext))
            {
                TempData["Error"] = "Only JPG, PNG and WebP images are allowed.";
                return RedirectToAction(nameof(Index));
            }

            var allowedMimeTypes = new[] { "image/jpeg", "image/png", "image/webp" };
            if (!allowedMimeTypes.Contains(imageFile.ContentType.ToLower()))
            {
                TempData["Error"] = "Invalid file type.";
                return RedirectToAction(nameof(Index));
            }

            if (imageFile.Length > 5 * 1024 * 1024)
            {
                TempData["Error"] = "Image must be under 5MB.";
                return RedirectToAction(nameof(Index));
            }

            var folder = Path.Combine(_environment.WebRootPath, "images", "Offers");
            Directory.CreateDirectory(folder);
            var fileName = Guid.NewGuid().ToString() + ext;
            var filePath = Path.Combine(folder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
                await imageFile.CopyToAsync(stream);

            var slide = new OfferSlide
            {
                ImagePath = "/images/Offers/" + fileName,
                Caption = caption?.Trim(),
                LinkUrl = linkUrl?.Trim(),
                DisplayOrder = displayOrder,
                IsActive = true,
                CreatedDate = TimeHelper.Now
            };
            _context.OfferSlides.Add(slide);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Offer slide uploaded successfully!";
            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/Offers/ToggleActive/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var slide = await _context.OfferSlides.FindAsync(id);
            if (slide != null)
            {
                slide.IsActive = !slide.IsActive;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/Offers/UpdateOrder/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateOrder(int id, int order)
        {
            var slide = await _context.OfferSlides.FindAsync(id);
            if (slide != null)
            {
                slide.DisplayOrder = order;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/Offers/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var slide = await _context.OfferSlides.FindAsync(id);
            if (slide != null)
            {
                // Delete physical file
                if (!string.IsNullOrEmpty(slide.ImagePath))
                {
                    var fullPath = Path.Combine(_environment.WebRootPath,
                        slide.ImagePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                    if (System.IO.File.Exists(fullPath))
                        System.IO.File.Delete(fullPath);
                }
                _context.OfferSlides.Remove(slide);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Slide deleted.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}