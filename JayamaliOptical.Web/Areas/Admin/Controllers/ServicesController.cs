using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JayamaliOptical.Web.Data;
using JayamaliOptical.Web.Models;

namespace JayamaliOptical.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ServicesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ServicesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var services = await _context.Services.OrderBy(s => s.DisplayOrder).ToListAsync();
            return View(services);
        }

        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Services/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Service service, IFormFile? ImageFile)
        {
            if (ModelState.IsValid)
            {
                // Handle image upload
                if (ImageFile != null)
                {
                    service.ImageUrl = await UploadServiceImageAsync(ImageFile);
                }
                service.Button1Url = ResolveButtonUrl(service.Button1Text, service.Button1Url);
                service.Button2Url = ResolveButtonUrl(service.Button2Text, service.Button2Url);
                service.Button3Url = ResolveButtonUrl(service.Button3Text, service.Button3Url);

                _context.Add(service);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(service);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var service = await _context.Services.FindAsync(id);
            if (service == null) return NotFound();
            return View(service);
        }

        // POST: Admin/Services/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Service service, IFormFile? ImageFile, bool removeImage = false)
        {
            if (id != service.Id) return NotFound();

            var existingService = await _context.Services.FindAsync(id);
            if (existingService == null) return NotFound();

            if (ModelState.IsValid)
            {
                // Handle image removal
                if (removeImage && !string.IsNullOrEmpty(existingService.ImageUrl))
                {
                    DeleteFile(existingService.ImageUrl);
                    existingService.ImageUrl = null;
                }

                // Handle new image upload
                if (ImageFile != null)
                {
                    if (!string.IsNullOrEmpty(existingService.ImageUrl))
                    {
                        DeleteFile(existingService.ImageUrl);
                    }
                    existingService.ImageUrl = await UploadServiceImageAsync(ImageFile);
                }

                // Update other properties
                existingService.Name = service.Name;
                existingService.Description = service.Description;
                existingService.Price = service.Price;
                existingService.DurationMinutes = service.DurationMinutes;
                existingService.RequiresAppointment = service.RequiresAppointment;
                existingService.Button1Text = service.Button1Text;
                existingService.Button1Url = ResolveButtonUrl(service.Button1Text, service.Button1Url);
                existingService.Button1Class = service.Button1Class;
                existingService.Button2Text = service.Button2Text;
                existingService.Button2Url = ResolveButtonUrl(service.Button2Text, service.Button2Url);
                existingService.Button2Class = service.Button2Class;
                existingService.Button3Text = service.Button3Text;
                existingService.Button3Url = ResolveButtonUrl(service.Button3Text, service.Button3Url);
                existingService.Button3Class = service.Button3Class;
                existingService.DisplayOrder = service.DisplayOrder;
                existingService.IsActive = service.IsActive;

                _context.Update(existingService);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(service);
        }

        // Helper method to upload service images
        private async Task<string> UploadServiceImageAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return string.Empty;

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "services");
            Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return "/uploads/services/" + uniqueFileName;
        }

        private void DeleteFile(string filePath)
        {
            if (!string.IsNullOrEmpty(filePath))
            {
                var physicalPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", filePath.TrimStart('/'));
                if (System.IO.File.Exists(physicalPath))
                {
                    System.IO.File.Delete(physicalPath);
                }
            }
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var service = await _context.Services.FirstOrDefaultAsync(m => m.Id == id);
            if (service == null) return NotFound();
            return View(service);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service != null)
            {
                _context.Services.Remove(service);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ServiceExists(int id)
        {
            return _context.Services.Any(e => e.Id == id);
        }

        // Always resolve URL from PredefinedButtons by matching button text
        // so the DB is never stuck with a stale/wrong URL
        private static string? ResolveButtonUrl(string? buttonText, string? fallbackUrl)
        {
            if (string.IsNullOrEmpty(buttonText)) return null;
            var match = PredefinedButtons.Buttons
                .FirstOrDefault(b => b.Text == buttonText);
            return match?.Url ?? fallbackUrl;
        }
    }
}