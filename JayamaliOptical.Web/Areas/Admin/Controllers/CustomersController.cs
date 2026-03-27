using JayamaliOptical.Web.Data;
using JayamaliOptical.Web.Models;
using JayamaliOptical.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JayamaliOptical.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CustomersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public CustomersController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(
            string search = "",
            string filter = "All",
            string sort = "newest",
            int page = 1)
        {
            int pageSize = 10;

            // ── Get admin emails to exclude everywhere ────────────────
            var adminUsers = await _userManager.GetUsersInRoleAsync("Admin");
            var adminEmails = adminUsers
                .Select(u => (u.Email ?? "").ToLower())
                .ToHashSet();

            // ── Orders grouped by email ───────────────────────────────
            var orderGroups = await _context.Orders
                .GroupBy(o => o.Email)
                .Select(g => new
                {
                    Email = g.Key,
                    FirstName = g.OrderByDescending(o => o.OrderDate).First().FirstName,
                    LastName = g.OrderByDescending(o => o.OrderDate).First().LastName,
                    PhoneNumber = g.OrderByDescending(o => o.OrderDate).First().PhoneNumber,
                    City = g.OrderByDescending(o => o.OrderDate).First().City,
                    TotalOrders = g.Count(),
                    TotalSpent = g.Sum(o => o.TotalAmount),
                    LastOrderDate = g.Max(o => o.OrderDate),
                    FirstOrderDate = g.Min(o => o.OrderDate)
                })
                .ToListAsync();

            // ── Bookings grouped by email ─────────────────────────────
            var bookingGroups = await _context.ServiceBookings
                .GroupBy(b => b.Email)
                .Select(g => new { Email = g.Key, Count = g.Count() })
                .ToListAsync();

            var bookingLookup = bookingGroups
                .ToDictionary(b => b.Email.ToLower(), b => b.Count);

            // booking-only = booking emails not in any order
            var bookingOnlyEmails = bookingGroups
                .Where(b => !orderGroups.Any(
                    o => o.Email.ToLower() == b.Email.ToLower()))
                .ToList();

            // ── Build customer list from orders ───────────────────────
            var customers = orderGroups
                .Where(g => !adminEmails.Contains(g.Email.ToLower()))
                .Select(g => new CustomerViewModel
                {
                    Email = g.Email,
                    FirstName = g.FirstName,
                    LastName = g.LastName,
                    PhoneNumber = g.PhoneNumber,
                    City = g.City,
                    TotalOrders = g.TotalOrders,
                    TotalBookings = bookingLookup.TryGetValue(
                                         g.Email.ToLower(), out var bc) ? bc : 0,
                    TotalSpent = g.TotalSpent,
                    LastOrderDate = g.LastOrderDate,
                    FirstOrderDate = g.FirstOrderDate,
                    IsRegistered = false
                }).ToList();

            // ── Add booking-only customers ────────────────────────────
            foreach (var b in bookingOnlyEmails
                .Where(b => !adminEmails.Contains(b.Email.ToLower())))
            {
                var latestBooking = await _context.ServiceBookings
                    .Where(sb => sb.Email.ToLower() == b.Email.ToLower())
                    .OrderByDescending(sb => sb.BookingDate)
                    .FirstAsync();

                customers.Add(new CustomerViewModel
                {
                    Email = b.Email,
                    FirstName = latestBooking.FirstName,
                    LastName = latestBooking.LastName,
                    PhoneNumber = latestBooking.PhoneNumber,
                    City = "",
                    TotalOrders = 0,
                    TotalBookings = b.Count,
                    TotalSpent = 0,
                    LastOrderDate = latestBooking.BookingDate,
                    FirstOrderDate = latestBooking.BookingDate,
                    IsRegistered = false
                });
            }

            // ── Add registered users who have no orders/bookings ──────
            // These are real customers who registered but haven't transacted yet
            var allUsers = await _userManager.Users.ToListAsync();
            var nonAdminUsers = allUsers
                .Where(u => u.Email != null
                         && !adminEmails.Contains(u.Email.ToLower()))
                .ToList();

            var existingEmails = customers
                .Select(c => c.Email.ToLower())
                .ToHashSet();

            foreach (var u in nonAdminUsers)
            {
                var uEmail = (u.Email ?? "").ToLower();
                if (!existingEmails.Contains(uEmail))
                {
                    // Registered user with no orders or bookings
                    customers.Add(new CustomerViewModel
                    {
                        Email = u.Email ?? "",
                        FirstName = u.UserName?.Split(' ').First() ?? "",
                        LastName = u.UserName?.Contains(' ') == true
                                         ? u.UserName.Split(' ', 2)[1] : "",
                        PhoneNumber = u.PhoneNumber ?? "",
                        City = "",
                        TotalOrders = 0,
                        TotalBookings = 0,
                        TotalSpent = 0,
                        LastOrderDate = DateTime.MinValue,
                        FirstOrderDate = DateTime.MinValue,
                        IsRegistered = true
                    });
                }
            }

            // ── Mark IsRegistered for all existing customers ──────────
            // Build normalised email → userId map for non-admin users
            var registeredEmailMap = nonAdminUsers
                .Where(u => u.NormalizedEmail != null)
                .ToDictionary(
                    u => u.NormalizedEmail!,
                    u => u.Id);

            foreach (var c in customers)
            {
                var key = c.Email.ToUpper();
                c.IsRegistered = registeredEmailMap.ContainsKey(key);
            }

            // ── Search ────────────────────────────────────────────────
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.ToLower();
                customers = customers.Where(c =>
                    c.Email.ToLower().Contains(s) ||
                    c.FirstName.ToLower().Contains(s) ||
                    c.LastName.ToLower().Contains(s) ||
                    c.PhoneNumber.Contains(s) ||
                    c.City.ToLower().Contains(s)
                ).ToList();
            }

            // ── Filter ────────────────────────────────────────────────
            customers = filter switch
            {
                "Registered" => customers.Where(c => c.IsRegistered).ToList(),
                "Guest" => customers.Where(c => !c.IsRegistered).ToList(),
                "HighValue" => customers.Where(c => c.TotalSpent >= 100000).ToList(),
                "Repeat" => customers.Where(c => c.TotalOrders >= 2).ToList(),
                "HasBookings" => customers.Where(c => c.TotalBookings > 0).ToList(),
                _ => customers
            };

            // ── Sort ──────────────────────────────────────────────────
            customers = sort switch
            {
                "oldest" => customers.OrderBy(c => c.FirstOrderDate).ToList(),
                "name" => customers.OrderBy(c => c.FirstName).ToList(),
                "spent" => customers.OrderByDescending(c => c.TotalSpent).ToList(),
                "orders" => customers.OrderByDescending(c => c.TotalOrders).ToList(),
                "bookings" => customers.OrderByDescending(c => c.TotalBookings).ToList(),
                _ => customers.OrderByDescending(c => c.LastOrderDate).ToList()
            };

            // ── Stats ─────────────────────────────────────────────────
            ViewBag.TotalCustomers = customers.Count;
            ViewBag.RegisteredCount = customers.Count(c => c.IsRegistered);
            ViewBag.GuestCount = customers.Count(c => !c.IsRegistered);
            ViewBag.HighValueCount = customers.Count(c => c.TotalSpent >= 100000);
            ViewBag.BookingCount = customers.Count(c => c.TotalBookings > 0);
            ViewBag.Search = search;
            ViewBag.Filter = filter;
            ViewBag.Sort = sort;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(
                                         customers.Count / (double)pageSize);

            return View(customers
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList());
        }

        public async Task<IActionResult> Details(string email)
        {
            if (string.IsNullOrEmpty(email)) return NotFound();

            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .Where(o => o.Email.ToLower() == email.ToLower())
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            var bookings = await _context.ServiceBookings
                .Include(b => b.Service)
                .Where(b => b.Email.ToLower() == email.ToLower())
                .OrderByDescending(b => b.AppointmentDate)
                .ToListAsync();

            // ── Find registered user first ────────────────────────────
            // Use NormalizedEmail for reliable case-insensitive match
            var normalizedEmail = email.ToUpper();
            var allUsers = await _userManager.Users.ToListAsync();
            var registeredUser = allUsers.FirstOrDefault(u =>
                (u.NormalizedEmail ?? "").Equals(
                    normalizedEmail, StringComparison.Ordinal));

            // If no orders and no bookings but user is registered — still show
            if (!orders.Any() && !bookings.Any() && registeredUser == null)
                return NotFound();

            string firstName, lastName, phoneNumber,
                   address = "", city = "", postalCode = "";
            DateTime firstDate = TimeHelper.Now, lastDate = TimeHelper.Now;

            if (orders.Any())
            {
                var latest = orders.First();
                firstName = latest.FirstName;
                lastName = latest.LastName;
                phoneNumber = latest.PhoneNumber;
                address = latest.Address;
                city = latest.City;
                postalCode = latest.PostalCode;
                firstDate = orders.Min(o => o.OrderDate);
                lastDate = orders.Max(o => o.OrderDate);
            }
            else if (bookings.Any())
            {
                var latestB = bookings.First();
                firstName = latestB.FirstName;
                lastName = latestB.LastName;
                phoneNumber = latestB.PhoneNumber;
                firstDate = bookings.Min(b => b.BookingDate);
                lastDate = bookings.Max(b => b.BookingDate);
            }
            else
            {
                // Registered user only — use Identity username
                firstName = registeredUser!.UserName?.Split(' ').First() ?? "";
                lastName = registeredUser.UserName?.Contains(' ') == true
                              ? registeredUser.UserName.Split(' ', 2)[1] : "";
                phoneNumber = registeredUser.PhoneNumber ?? "";
            }

            // ── Load prescriptions via UserId ─────────────────────────
            var prescriptions = new List<Prescription>();
            if (registeredUser != null)
            {
                prescriptions = await _context.Prescriptions
                    .Where(p => p.UserId == registeredUser.Id)
                    .OrderByDescending(p => p.CreatedDate)
                    .ToListAsync();
            }

            var vm = new CustomerDetailViewModel
            {
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                PhoneNumber = phoneNumber,
                Address = address,
                City = city,
                PostalCode = postalCode,
                IsRegistered = registeredUser != null,
                RegisteredUserId = registeredUser?.Id,
                Orders = orders,
                Bookings = bookings,
                Prescriptions = prescriptions,
                TotalSpent = orders.Sum(o => o.TotalAmount),
                FirstOrderDate = firstDate,
                LastOrderDate = lastDate
            };

            return View(vm);
        }
    }
}