using JayamaliOptical.Web.Data;
using JayamaliOptical.Web.Models;
using JayamaliOptical.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JayamaliOptical.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string status = "All", int? page = 1)
        {
            var orders = _context.Orders.Include(o => o.OrderItems).AsQueryable();

            if (!string.IsNullOrEmpty(status) && status != "All")
                orders = orders.Where(o => o.Status == status);

            orders = orders.OrderByDescending(o => o.OrderDate);

            int pageSize = 10;
            var totalPages = (int)Math.Ceiling(await orders.CountAsync() / (double)pageSize);
            var paginatedOrders = await orders.Skip((page!.Value - 1) * pageSize).Take(pageSize).ToListAsync();

            ViewBag.StatusFilter = status;
            ViewBag.CurrentPage = page ?? 1;
            ViewBag.TotalPages = totalPages;
            ViewBag.Statuses = new[] { "All", "Pending", "Confirmed", "Processing", "Shipped", "Delivered", "Cancelled" };

            return View(paginatedOrders);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.Prescription)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            return View(order);
        }

        public async Task<IActionResult> UpdateStatus(int? id)
        {
            if (id == null) return NotFound();

            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();

            ViewBag.Statuses = new[] { "Pending", "Confirmed", "Processing", "Shipped", "Delivered", "Cancelled" };
            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string? status)
        {
            if (string.IsNullOrEmpty(status)) return BadRequest("Status is required");

            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();

            order.Status = status;
            if (status == "Shipped" && order.ShippedDate == null)
                order.ShippedDate = TimeHelper.Now;
            else if (status == "Delivered" && order.DeliveredDate == null)
                order.DeliveredDate = TimeHelper.Now;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> PrintInvoice(int? id)
        {
            if (id == null) return NotFound();

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.Prescription)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            return View(order);
        }

        // GET: Admin/Orders/CustomerInvoice/5
        public async Task<IActionResult> CustomerInvoice(int? id)
        {
            if (id == null) return NotFound();

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            return View(order);
        }
    }
}