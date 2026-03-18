using JayamaliOptical.Web.Data;
using JayamaliOptical.Web.Models;
using JayamaliOptical.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;  // ADD THIS
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace JayamaliOptical.Web.Controllers
{
    public class BookingsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;  // ADD THIS
        private readonly ILogger<BookingsController> _logger;

        public BookingsController(
            ApplicationDbContext context,
            IEmailService emailService,
            IConfiguration configuration,  // ADD THIS
            ILogger<BookingsController> logger = null)
        {
            _context = context;
            _emailService = emailService;
            _configuration = configuration;  // ADD THIS
            _logger = logger;
        }

        // GET: Bookings/Create/5
        public async Task<IActionResult> Create(int? serviceId)
        {
            var model = new ServiceBookingViewModel();

            if (serviceId.HasValue)
            {
                var service = await _context.Services.FindAsync(serviceId.Value);
                if (service != null)
                {
                    model.ServiceId = service.Id;
                    model.ServiceName = service.Name;
                }
            }

            ViewBag.Services = await _context.Services
                .Where(s => s.IsActive && s.RequiresAppointment)
                .OrderBy(s => s.Name)
                .ToListAsync();

            ViewBag.TimeSlots = GetAvailableTimeSlots();

            return View(model);
        }

        // POST: Bookings/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ServiceBookingViewModel model)
        {
            ViewBag.Services = await _context.Services
                .Where(s => s.IsActive && s.RequiresAppointment)
                .OrderBy(s => s.Name)
                .ToListAsync();

            ViewBag.TimeSlots = GetAvailableTimeSlots();

            if (ModelState.IsValid)
            {
                // Parse time
                if (!TimeSpan.TryParse(model.AppointmentTime, out var appointmentTime))
                {
                    ModelState.AddModelError("AppointmentTime", "Invalid time format");
                    return View(model);
                }

                // Create booking
                var booking = new ServiceBooking
                {
                    BookingNumber = GenerateBookingNumber(),
                    ServiceId = model.ServiceId,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    AppointmentDate = model.AppointmentDate.Date,
                    AppointmentTime = appointmentTime,
                    Notes = model.Notes,
                    BookingDate = DateTime.Now,
                    Status = "Pending",
                    UserId = User.FindFirstValue(ClaimTypes.NameIdentifier)  // ← ADD THIS
                };

                _context.ServiceBookings.Add(booking);
                await _context.SaveChangesAsync();

                // Send booking confirmation email
                try
                {
                    var service = await _context.Services.FindAsync(booking.ServiceId);

                    await _emailService.SendBookingConfirmationAsync(
                        booking.Email,
                        $"{booking.FirstName} {booking.LastName}",
                        booking.BookingNumber,
                        service?.Name ?? "Service",
                        booking.AppointmentDate,
                        booking.AppointmentTime
                    );
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Email sending failed");
                }

                return RedirectToAction("Confirmation", new { bookingId = booking.Id });
            }

            return View(model);
        }

        // GET: Bookings/Confirmation/5
        public async Task<IActionResult> Confirmation(int? bookingId)
        {
            if (bookingId == null)
            {
                return NotFound();
            }

            var booking = await _context.ServiceBookings
                .Include(b => b.Service)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        // Get available time slots
        private List<string> GetAvailableTimeSlots()
        {
            return new List<string>
            {
                "09:00", "09:30", "10:00", "10:30",
                "11:00", "11:30",
                "14:00", "14:30", "15:00", "15:30",
                "16:00", "16:30", "17:00"
            };
        }

        // Generate unique booking number
        private string GenerateBookingNumber()
        {
            return "BKG-" + DateTime.Now.ToString("yyyyMMddHHmmss") + "-" + new Random().Next(1000, 9999);
        }
    }
}