using JayamaliOptical.Web.Data;
using JayamaliOptical.Web.Models;
using JayamaliOptical.Web.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace JayamaliOptical.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;

        public HomeController(ApplicationDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        private async Task<SiteSettings> GetSettingsAsync() =>
            await _context.SiteSettings.FirstOrDefaultAsync() ?? new SiteSettings();

        public async Task<IActionResult> Index()
        {
            ViewBag.Services = await _context.Services
                .Where(s => s.IsActive).OrderBy(s => s.DisplayOrder).ToListAsync();
            ViewBag.DefaultService = await _context.Services
                .Where(s => s.IsActive && s.DisplayOrder == 1).FirstOrDefaultAsync()
                ?? await _context.Services.Where(s => s.IsActive).OrderBy(s => s.DisplayOrder).FirstOrDefaultAsync();
            ViewBag.Settings = await GetSettingsAsync();
            ViewBag.OfferSlides = await _context.OfferSlides
                .Where(o => o.IsActive)
                .OrderBy(o => o.DisplayOrder)
                .ToListAsync();
            return View();
        }

        public async Task<IActionResult> ContactUs()
        {
            ViewBag.Settings = await GetSettingsAsync();
            return View();
        }

        public async Task<IActionResult> AboutUs()
        {
            ViewBag.Settings = await GetSettingsAsync();
            return View();
        }

        public async Task<IActionResult> Privacy()
        {
            ViewBag.Settings = await GetSettingsAsync();
            return View();
        }

        // POST: Contact form submission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendMessage(string name, string phone, string email, string subject, string message)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(message))
            {
                TempData["ContactError"] = "Please fill in all required fields.";
                return RedirectToAction(nameof(ContactUs));
            }

            // 1. Save to database
            var contact = new ContactMessage
            {
                Name = name.Trim(),
                Phone = phone?.Trim(),
                Email = email.Trim(),
                Subject = subject?.Trim(),
                Message = message.Trim(),
                SentAt = DateTime.Now,
                IsRead = false
            };
            _context.ContactMessages.Add(contact);
            await _context.SaveChangesAsync();

            // 2. Send email notification (best effort)
            await _emailService.SendContactMessageAsync(name, email, phone ?? "", subject ?? "", message);

            TempData["ContactSuccess"] = "Thank you! Your message has been sent. We'll get back to you shortly.";
            return RedirectToAction(nameof(ContactUs));
        }

        [HttpGet]
        public async Task<IActionResult> GetServiceDetails(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service == null) return NotFound();
            return Json(new
            {
                id = service.Id,
                name = service.Name,
                description = service.Description,
                price = service.Price,
                durationMinutes = service.DurationMinutes,
                requiresAppointment = service.RequiresAppointment,
                imageUrl = service.ImageUrl,
                button1Text = service.Button1Text,
                button1Url = service.Button1Url,
                button1Class = service.Button1Class,
                button2Text = service.Button2Text,
                button2Url = service.Button2Url,
                button2Class = service.Button2Class,
                button3Text = service.Button3Text,
                button3Url = service.Button3Url,
                button3Class = service.Button3Class
            });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() =>
            View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}