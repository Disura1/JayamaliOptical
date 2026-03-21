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

        // GET
        public async Task<IActionResult> Index()
        {
            var settings = await _context.SiteSettings.FirstOrDefaultAsync()
                           ?? new SiteSettings();
            return View(settings);
        }

        // POST — reads form directly to avoid checkbox/disabled input binding issues
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Index")]
        public async Task<IActionResult> IndexPost()
        {
            var f = Request.Form;

            var existing = await _context.SiteSettings.FirstOrDefaultAsync();
            if (existing == null)
            {
                existing = new SiteSettings();
                _context.SiteSettings.Add(existing);
            }

            // Contact
            existing.Address = f["Address"].ToString().Trim();
            existing.Phone1 = f["Phone1"].ToString().Trim();
            existing.Phone2 = f["Phone2"].ToString().Trim();
            existing.Phone3 = f["Phone3"].ToString().Trim();
            existing.Email = f["Email"].ToString().Trim();

            // Social
            existing.FacebookUrl = f["FacebookUrl"].ToString().Trim();
            existing.WhatsAppUrl = f["WhatsAppUrl"].ToString().Trim();
            existing.TikTokUrl = f["TikTokUrl"].ToString().Trim();
            existing.InstagramUrl = f["InstagramUrl"].ToString().Trim();

            // Hours — unchecked checkboxes send nothing, so Contains("true") = false naturally
            existing.MonOpen = f["MonOpen"].FirstOrDefault() ?? "08:30";
            existing.MonClose = f["MonClose"].FirstOrDefault() ?? "18:00";
            existing.MonClosed = f["MonClosed"].ToString().Contains("true");

            existing.TueOpen = f["TueOpen"].FirstOrDefault() ?? "08:30";
            existing.TueClose = f["TueClose"].FirstOrDefault() ?? "18:00";
            existing.TueClosed = f["TueClosed"].ToString().Contains("true");

            existing.WedOpen = f["WedOpen"].FirstOrDefault() ?? "08:30";
            existing.WedClose = f["WedClose"].FirstOrDefault() ?? "18:00";
            existing.WedClosed = f["WedClosed"].ToString().Contains("true");

            existing.ThuOpen = f["ThuOpen"].FirstOrDefault() ?? "08:30";
            existing.ThuClose = f["ThuClose"].FirstOrDefault() ?? "18:00";
            existing.ThuClosed = f["ThuClosed"].ToString().Contains("true");

            existing.FriOpen = f["FriOpen"].FirstOrDefault() ?? "08:30";
            existing.FriClose = f["FriClose"].FirstOrDefault() ?? "18:00";
            existing.FriClosed = f["FriClosed"].ToString().Contains("true");

            existing.SatOpen = f["SatOpen"].FirstOrDefault() ?? "08:30";
            existing.SatClose = f["SatClose"].FirstOrDefault() ?? "16:00";
            existing.SatClosed = f["SatClosed"].ToString().Contains("true");

            existing.SunOpen = f["SunOpen"].FirstOrDefault() ?? "08:30";
            existing.SunClose = f["SunClose"].FirstOrDefault() ?? "18:00";
            existing.SunClosed = f["SunClosed"].ToString().Contains("true");

            existing.HolOpen = f["HolOpen"].FirstOrDefault() ?? "08:30";
            existing.HolClose = f["HolClose"].FirstOrDefault() ?? "18:00";
            existing.HolClosed = f["HolClosed"].ToString().Contains("true");

            // Content
            existing.PrivacyPolicy = f["PrivacyPolicy"].ToString();
            existing.History = f["History"].ToString();
            existing.Vision = f["Vision"].ToString();
            existing.Mission = f["Mission"].ToString();

            await _context.SaveChangesAsync();
            TempData["Success"] = "Site settings saved successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}