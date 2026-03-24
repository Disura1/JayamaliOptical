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

        public SiteSettingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Admin/SiteSettings
        public async Task<IActionResult> Index()
        {
            var settings = await _context.SiteSettings.FirstOrDefaultAsync()
                           ?? new SiteSettings();
            return View(settings);
        }

        // POST: /Admin/SiteSettings/Save
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save()
        {
            try
            {
                var f = Request.Form;

                var existing = await _context.SiteSettings.FirstOrDefaultAsync();

                if (existing == null)
                {
                    existing = new SiteSettings();
                    _context.SiteSettings.Add(existing);
                }

                // Contact
                existing.Address = f["Address"].FirstOrDefault() ?? "";
                existing.Phone1 = f["Phone1"].FirstOrDefault() ?? "";
                existing.Phone2 = f["Phone2"].FirstOrDefault() ?? "";
                existing.Phone3 = f["Phone3"].FirstOrDefault() ?? "";
                existing.Email = f["Email"].FirstOrDefault() ?? "";

                // Social
                existing.FacebookUrl = f["FacebookUrl"].FirstOrDefault() ?? "";
                existing.WhatsAppUrl = f["WhatsAppUrl"].FirstOrDefault() ?? "";
                existing.TikTokUrl = f["TikTokUrl"].FirstOrDefault() ?? "";
                existing.InstagramUrl = f["InstagramUrl"].FirstOrDefault() ?? "";

                // Hours
                existing.MonOpen = f["MonOpen"].FirstOrDefault() ?? "08:30";
                existing.MonClose = f["MonClose"].FirstOrDefault() ?? "18:00";
                existing.MonClosed = f["MonClosed"].FirstOrDefault() == "true";

                existing.TueOpen = f["TueOpen"].FirstOrDefault() ?? "08:30";
                existing.TueClose = f["TueClose"].FirstOrDefault() ?? "18:00";
                existing.TueClosed = f["TueClosed"].FirstOrDefault() == "true";

                existing.WedOpen = f["WedOpen"].FirstOrDefault() ?? "08:30";
                existing.WedClose = f["WedClose"].FirstOrDefault() ?? "18:00";
                existing.WedClosed = f["WedClosed"].FirstOrDefault() == "true";

                existing.ThuOpen = f["ThuOpen"].FirstOrDefault() ?? "08:30";
                existing.ThuClose = f["ThuClose"].FirstOrDefault() ?? "18:00";
                existing.ThuClosed = f["ThuClosed"].FirstOrDefault() == "true";

                existing.FriOpen = f["FriOpen"].FirstOrDefault() ?? "08:30";
                existing.FriClose = f["FriClose"].FirstOrDefault() ?? "18:00";
                existing.FriClosed = f["FriClosed"].FirstOrDefault() == "true";

                existing.SatOpen = f["SatOpen"].FirstOrDefault() ?? "08:30";
                existing.SatClose = f["SatClose"].FirstOrDefault() ?? "16:00";
                existing.SatClosed = f["SatClosed"].FirstOrDefault() == "true";

                existing.SunOpen = f["SunOpen"].FirstOrDefault() ?? "08:30";
                existing.SunClose = f["SunClose"].FirstOrDefault() ?? "18:00";
                existing.SunClosed = f["SunClosed"].FirstOrDefault() == "true";

                existing.HolOpen = f["HolOpen"].FirstOrDefault() ?? "08:30";
                existing.HolClose = f["HolClose"].FirstOrDefault() ?? "18:00";
                existing.HolClosed = f["HolClosed"].FirstOrDefault() == "true";

                // Content
                existing.PrivacyPolicy = f["PrivacyPolicy"].FirstOrDefault() ?? "";
                existing.History = f["History"].FirstOrDefault() ?? "";
                existing.Vision = f["Vision"].FirstOrDefault() ?? "";
                existing.Mission = f["Mission"].FirstOrDefault() ?? "";

                // Payment
                existing.CodEnabled = f["CodEnabled"].FirstOrDefault() == "true";
                existing.PayHereEnabled = f["PayHereEnabled"].FirstOrDefault() == "true";
                existing.PayHereMerchantId = f["PayHereMerchantId"].FirstOrDefault() ?? string.Empty;
                existing.PayHereSandbox = f["PayHereSandbox"].FirstOrDefault() == "true";

                // Only update secret if a new value was provided (don't wipe it with empty on save)
                var newSecret = f["PayHereMerchantSecret"].FirstOrDefault() ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(newSecret))
                    existing.PayHereMerchantSecret = newSecret;

                await _context.SaveChangesAsync();
                TempData["Success"] = "Site settings saved successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error saving settings: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}