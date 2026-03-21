using JayamaliOptical.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using JayamaliOptical.Web.Data;

namespace JayamaliOptical.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        private async Task<SiteSettings> GetSettingsAsync()
        {
            return await _context.SiteSettings.FirstOrDefaultAsync()
                   ?? new SiteSettings();
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.Services = await _context.Services
                .Where(s => s.IsActive).OrderBy(s => s.DisplayOrder).ToListAsync();
            ViewBag.DefaultService = await _context.Services
                .Where(s => s.IsActive && s.DisplayOrder == 1).FirstOrDefaultAsync()
                ?? await _context.Services.Where(s => s.IsActive).OrderBy(s => s.DisplayOrder).FirstOrDefaultAsync();
            ViewBag.Settings = await GetSettingsAsync();
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
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}