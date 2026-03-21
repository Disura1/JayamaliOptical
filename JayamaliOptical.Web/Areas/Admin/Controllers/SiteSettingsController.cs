using JayamaliOptical.Web.Data;
using JayamaliOptical.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JayamaliOptical.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class SiteSettingsController : Controller
    {
        private readonly ApplicationDbContext _context;
        public SiteSettingsController(ApplicationDbContext context) => _context = context;

        public async Task<IActionResult> Index()
        {
            var settings = await _context.SiteSettings.FirstOrDefaultAsync()
                           ?? new SiteSettings();
            return View(settings);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(SiteSettings model)
        {
            if (!ModelState.IsValid) return View(model);

            var existing = await _context.SiteSettings.FirstOrDefaultAsync();
            if (existing == null)
            {
                _context.SiteSettings.Add(model);
            }
            else
            {
                existing.Address = model.Address;
                existing.Phone1 = model.Phone1;
                existing.Phone2 = model.Phone2;
                existing.Phone3 = model.Phone3;
                existing.Email = model.Email;
                existing.FacebookUrl = model.FacebookUrl;
                existing.WhatsAppUrl = model.WhatsAppUrl;
                existing.TikTokUrl = model.TikTokUrl;
                existing.InstagramUrl = model.InstagramUrl;
                existing.PrivacyPolicy = model.PrivacyPolicy;
                existing.History = model.History;
                existing.Vision = model.Vision;
                existing.Mission = model.Mission;
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Site settings saved successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}