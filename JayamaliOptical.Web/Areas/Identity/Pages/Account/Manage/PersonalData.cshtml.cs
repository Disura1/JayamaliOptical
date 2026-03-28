#nullable disable
using JayamaliOptical.Web.Data;
using JayamaliOptical.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;

namespace JayamaliOptical.Web.Areas.Identity.Pages.Account.Manage
{
    public class PersonalDataModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _context;

        [TempData]
        public string StatusMessage { get; set; }

        public PersonalDataModel(
            UserManager<IdentityUser> userManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();
            return Page();
        }

        public async Task<IActionResult> OnPostDownloadDataAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            // Core identity data
            var personalData = new Dictionary<string, object>
            {
                ["Account"] = new
                {
                    user.Id,
                    user.UserName,
                    user.Email,
                    user.EmailConfirmed,
                    user.PhoneNumber,
                    user.TwoFactorEnabled,
                    user.LockoutEnabled
                }
            };

            // Orders
            var orders = await _context.Orders
                .Where(o => o.Email == user.Email || o.UserId == user.Id)
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new
                {
                    o.OrderNumber,
                    o.OrderDate,
                    o.Status,
                    o.TotalAmount,
                    o.Address,
                    o.City,
                    o.PostalCode
                })
                .ToListAsync();
            personalData["Orders"] = orders;

            // Bookings
            var bookings = await _context.ServiceBookings
                .Include(b => b.Service)
                .Where(b => b.Email == user.Email || b.UserId == user.Id)
                .OrderByDescending(b => b.AppointmentDate)
                .Select(b => new
                {
                    b.BookingNumber,
                    Service = b.Service != null ? b.Service.Name : "N/A",
                    b.AppointmentDate,
                    AppointmentTime = b.AppointmentTime.ToString(),
                    b.Status,
                    b.FirstName,
                    b.LastName,
                    b.PhoneNumber,
                    b.Notes
                })
                .ToListAsync();
            personalData["Appointments"] = bookings;

            var json = JsonSerializer.Serialize(personalData, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            var fileName = $"JayamaliOptical-PersonalData-{user.UserName}-{TimeHelper.Now:yyyyMMdd}.json";
            return File(Encoding.UTF8.GetBytes(json), "application/json", fileName);
        }
    }
}