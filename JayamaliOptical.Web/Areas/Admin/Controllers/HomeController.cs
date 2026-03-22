using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
        private readonly UserManager<IdentityUser> _userManager;

        public HomeController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var today = DateTime.Today;
            var monthStart = new DateTime(today.Year, today.Month, 1);

            // ── Customer count — exactly mirrors CustomersController ───
            // Step 1: admin emails to exclude
            var adminUsers = await _userManager.GetUsersInRoleAsync("Admin");
            var adminEmails = adminUsers
                .Select(u => (u.Email ?? "").ToLower())
                .ToHashSet();

            // Step 2: unique order emails (excluding admins)
            var orderEmails = await _context.Orders
                .Select(o => o.Email.ToLower())
                .Distinct()
                .ToListAsync();
            orderEmails = orderEmails
                .Where(e => !adminEmails.Contains(e))
                .ToList();

            // Step 3: booking-only emails (not in orders, not admin)
            var allBookingEmails = await _context.ServiceBookings
                .Select(b => b.Email.ToLower())
                .Distinct()
                .ToListAsync();
            var bookingOnlyEmails = allBookingEmails
                .Where(e => !orderEmails.Contains(e)
                         && !adminEmails.Contains(e))
                .ToList();

            // Step 4: registered users who have no orders or bookings
            var allUsers = await _userManager.Users.ToListAsync();
            var allTransactionEmails = orderEmails
                .Union(bookingOnlyEmails)
                .ToHashSet();
            var registeredOnlyCount = allUsers
                .Where(u => u.Email != null
                         && !adminEmails.Contains(u.Email.ToLower())
                         && !allTransactionEmails.Contains(u.Email.ToLower()))
                .Count();

            // Total = orders + booking-only + registered-only
            var totalCustomers = orderEmails.Count
                               + bookingOnlyEmails.Count
                               + registeredOnlyCount;

            var model = new DashboardViewModel
            {
                TotalCustomers = totalCustomers,

                // Orders
                TotalOrders = await _context.Orders.CountAsync(),
                PendingOrders = await _context.Orders
                    .CountAsync(o => o.Status == "Pending"),
                DeliveredOrders = await _context.Orders
                    .CountAsync(o => o.Status == "Delivered"),
                TotalRevenue = await _context.Orders
                    .Where(o => o.Status != "Cancelled")
                    .SumAsync(o => (decimal?)o.TotalAmount) ?? 0,
                MonthRevenue = await _context.Orders
                    .Where(o => o.OrderDate >= monthStart
                             && o.Status != "Cancelled")
                    .SumAsync(o => (decimal?)o.TotalAmount) ?? 0,

                // Appointments
                TotalAppointments = await _context.ServiceBookings.CountAsync(),
                TodayAppointments = await _context.ServiceBookings
                    .CountAsync(b => b.AppointmentDate.Date == today),
                PendingAppointments = await _context.ServiceBookings
                    .CountAsync(b => b.Status == "Pending"),
                UpcomingAppointments = await _context.ServiceBookings
                    .CountAsync(b => b.AppointmentDate.Date >= today
                                  && b.Status != "Cancelled"),

                // Products
                TotalProducts = await _context.Products.CountAsync(),
                ActiveProducts = await _context.Products
                    .CountAsync(p => p.IsActive),

                // Reviews & Messages
                TotalReviews = await _context.Reviews
                    .CountAsync(r => r.IsApproved),
                PendingReviews = await _context.Reviews
                    .CountAsync(r => !r.IsApproved),
                UnreadMessages = await _context.ContactMessages
                    .CountAsync(m => !m.IsRead),

                // Recent orders — last 5
                RecentOrders = await _context.Orders
                    .OrderByDescending(o => o.OrderDate)
                    .Take(5)
                    .ToListAsync(),

                // Upcoming bookings — next 5
                UpcomingBookings = await _context.ServiceBookings
                    .Include(b => b.Service)
                    .Where(b => b.AppointmentDate.Date >= today
                             && b.Status != "Cancelled")
                    .OrderBy(b => b.AppointmentDate)
                    .ThenBy(b => b.AppointmentTime)
                    .Take(5)
                    .ToListAsync()
            };

            return View(model);
        }
    }
}