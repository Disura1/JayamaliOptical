using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using JayamaliOptical.Web.Models;
using JayamaliOptical.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace JayamaliOptical.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var today = DateTime.Today;
            var monthStart = new DateTime(today.Year, today.Month, 1);

            // Match exactly how CustomersController counts — unique emails from orders + booking-only emails
            var orderEmailsList = await _context.Orders
                .Select(o => o.Email.ToLower())
                .Distinct()
                .ToListAsync();

            var bookingOnlyEmailsList = await _context.ServiceBookings
                .Select(b => b.Email.ToLower())
                .Distinct()
                .ToListAsync();

            // booking-only = booking emails that don't appear in any order
            var bookingOnlyUnique = bookingOnlyEmailsList
                .Where(e => !orderEmailsList.Contains(e))
                .ToList();

            var totalCustomers = orderEmailsList.Count + bookingOnlyUnique.Count;

            var model = new DashboardViewModel
            {
                TotalCustomers = totalCustomers,

                // Orders
                TotalOrders = await _context.Orders.CountAsync(),
                PendingOrders = await _context.Orders.CountAsync(o => o.Status == "Pending"),
                DeliveredOrders = await _context.Orders.CountAsync(o => o.Status == "Delivered"),
                TotalRevenue = await _context.Orders
                    .Where(o => o.Status != "Cancelled")
                    .SumAsync(o => (decimal?)o.TotalAmount) ?? 0,
                MonthRevenue = await _context.Orders
                    .Where(o => o.OrderDate >= monthStart && o.Status != "Cancelled")
                    .SumAsync(o => (decimal?)o.TotalAmount) ?? 0,

                // Appointments
                TotalAppointments = await _context.ServiceBookings.CountAsync(),
                TodayAppointments = await _context.ServiceBookings
                    .CountAsync(b => b.AppointmentDate.Date == today),
                PendingAppointments = await _context.ServiceBookings
                    .CountAsync(b => b.Status == "Pending"),
                UpcomingAppointments = await _context.ServiceBookings
                    .CountAsync(b => b.AppointmentDate.Date >= today && b.Status != "Cancelled"),

                // Products
                TotalProducts = await _context.Products.CountAsync(),
                ActiveProducts = await _context.Products.CountAsync(p => p.IsActive),

                // Reviews & Messages
                TotalReviews = await _context.Reviews.CountAsync(r => r.IsApproved),
                PendingReviews = await _context.Reviews.CountAsync(r => !r.IsApproved),
                UnreadMessages = await _context.ContactMessages.CountAsync(m => !m.IsRead),

                // Recent orders — last 5
                RecentOrders = await _context.Orders
                    .OrderByDescending(o => o.OrderDate)
                    .Take(5)
                    .ToListAsync(),

                // Upcoming bookings — next 5
                UpcomingBookings = await _context.ServiceBookings
                    .Include(b => b.Service)
                    .Where(b => b.AppointmentDate.Date >= today && b.Status != "Cancelled")
                    .OrderBy(b => b.AppointmentDate)
                    .ThenBy(b => b.AppointmentTime)
                    .Take(5)
                    .ToListAsync()
            };

            return View(model);
        }
    }
}