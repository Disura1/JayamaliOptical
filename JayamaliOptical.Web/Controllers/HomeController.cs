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

        // ADD THIS CONSTRUCTOR
        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // UPDATE THIS INDEX METHOD
        public async Task<IActionResult> Index()
        {
            // Get all active services ordered by DisplayOrder
            ViewBag.Services = await _context.Services
                .Where(s => s.IsActive)
                .OrderBy(s => s.DisplayOrder)
                .ToListAsync();

            // Get first service (DisplayOrder = 1) as default
            ViewBag.DefaultService = await _context.Services
                .Where(s => s.IsActive && s.DisplayOrder == 1)
                .FirstOrDefaultAsync();

            // If no service with DisplayOrder=1, get the first one
            if (ViewBag.DefaultService == null)
            {
                ViewBag.DefaultService = await _context.Services
                    .Where(s => s.IsActive)
                    .OrderBy(s => s.DisplayOrder)
                    .FirstOrDefaultAsync();
            }

            return View();
        }

        public IActionResult ContactUs()
        {
            return View();
        }

        public IActionResult AboutUs()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        // ADD THIS NEW METHOD FOR API
        [HttpGet]
        public async Task<IActionResult> GetServiceDetails(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service == null)
            {
                return NotFound();
            }

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