using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JayamaliOptical.Web.Data;
using System.Security.Claims;

namespace JayamaliOptical.Web.Controllers
{
    [Authorize]  // Require login
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Dashboard
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Get user's orders
            var orders = await _context.Orders
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            // Get user's bookings
            var bookings = await _context.ServiceBookings
                .Include(b => b.Service)
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.AppointmentDate)
                .ToListAsync();

            var dashboardModel = new DashboardViewModel
            {
                Orders = orders,
                Bookings = bookings
            };

            return View(dashboardModel);
        }

        // GET: Dashboard/OrderDetails/5
        public async Task<IActionResult> OrderDetails(int? id)
        {
            if (id == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

            if (order == null) return NotFound();

            return View(order);
        }

        // GET: Dashboard/BookingDetails/5
        public async Task<IActionResult> BookingDetails(int? id)
        {
            if (id == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var booking = await _context.ServiceBookings
                .Include(b => b.Service)
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

            if (booking == null) return NotFound();

            return View(booking);
        }
    }

    public class DashboardViewModel
    {
        public List<JayamaliOptical.Web.Models.Order> Orders { get; set; } = new();
        public List<JayamaliOptical.Web.Models.ServiceBooking> Bookings { get; set; } = new();
    }
}