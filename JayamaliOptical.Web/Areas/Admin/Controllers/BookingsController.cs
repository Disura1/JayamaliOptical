using JayamaliOptical.Web.Data;
using JayamaliOptical.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JayamaliOptical.Web.Services;

namespace JayamaliOptical.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class BookingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BookingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string status = "All", string date = "", int page = 1)
        {
            var bookings = _context.ServiceBookings
                .Include(b => b.Service)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status) && status != "All")
                bookings = bookings.Where(b => b.Status == status);

            if (!string.IsNullOrEmpty(date) && DateTime.TryParse(date, out var filterDate))
                bookings = bookings.Where(b => b.AppointmentDate.Date == filterDate.Date);

            bookings = bookings.OrderByDescending(b => b.AppointmentDate)
                               .ThenBy(b => b.AppointmentTime);

            int pageSize = 15;
            var total = await bookings.CountAsync();
            var totalPages = (int)Math.Ceiling(total / (double)pageSize);
            var paged = await bookings.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            ViewBag.StatusFilter = status;
            ViewBag.DateFilter = date;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalCount = total;
            ViewBag.Statuses = new[] { "All", "Pending", "Confirmed", "Completed", "Cancelled", "NoShow" };

            // Summary counts
            ViewBag.PendingCount = await _context.ServiceBookings.CountAsync(b => b.Status == "Pending");

            var todaySriLanka = TimeHelper.Today;
            ViewBag.TodayCount = await _context.ServiceBookings.CountAsync(b => b.AppointmentDate.Date == todaySriLanka);

            ViewBag.ConfirmedCount = await _context.ServiceBookings.CountAsync(b => b.Status == "Confirmed");

            return View(paged);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var booking = await _context.ServiceBookings
                .Include(b => b.Service)
                .FirstOrDefaultAsync(b => b.Id == id);
            if (booking == null) return NotFound();
            return View(booking);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status, string? adminNotes)
        {
            var booking = await _context.ServiceBookings.FindAsync(id);
            if (booking == null) return NotFound();

            booking.Status = status;
            if (!string.IsNullOrWhiteSpace(adminNotes))
                booking.AdminNotes = adminNotes;

            await _context.SaveChangesAsync();
            TempData["Success"] = $"Booking {booking.BookingNumber} status updated to {status}.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var booking = await _context.ServiceBookings.FindAsync(id);
            if (booking != null)
            {
                _context.ServiceBookings.Remove(booking);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Booking deleted.";
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> AppointmentInvoice(int? id)
        {
            if (id == null) return NotFound();
            var booking = await _context.ServiceBookings
                .Include(b => b.Service)
                .FirstOrDefaultAsync(b => b.Id == id);
            if (booking == null) return NotFound();
            return View(booking);
        }
    }
}