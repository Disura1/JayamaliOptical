using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JayamaliOptical.Web.Data;
using JayamaliOptical.Web.Models;

namespace JayamaliOptical.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Orders
        public async Task<IActionResult> Index(string status = "All", int? page = 1)
        {
            var orders = _context.Orders.Include(o => o.OrderItems).AsQueryable();

            // Filter by status
            if (!string.IsNullOrEmpty(status) && status != "All")
            {
                orders = orders.Where(o => o.Status == status);
            }

            // Order by date (newest first)
            orders = orders.OrderByDescending(o => o.OrderDate);

            // Pagination (10 orders per page)
            int pageSize = 10;
            var totalPages = (int)Math.Ceiling(await orders.CountAsync() / (double)pageSize);
            var paginatedOrders = await orders.Skip((page.Value - 1) * pageSize).Take(pageSize).ToListAsync();

            ViewBag.StatusFilter = status;
            ViewBag.CurrentPage = page ?? 1;
            ViewBag.TotalPages = totalPages;
            ViewBag.Statuses = new[] { "All", "Pending", "Confirmed", "Processing", "Shipped", "Delivered", "Cancelled" };

            return View(paginatedOrders);
        }

        // GET: Admin/Orders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // GET: Admin/Orders/UpdateStatus/5
        public async Task<IActionResult> UpdateStatus(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            ViewBag.Statuses = new[] { "Pending", "Confirmed", "Processing", "Shipped", "Delivered", "Cancelled" };
            return View(order);
        }

        // POST: Admin/Orders/UpdateStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string? status)
        {
            if (string.IsNullOrEmpty(status))
            {
                return BadRequest("Status is required");
            }

            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            order.Status = status;

            // Update timestamps based on status
            if (status == "Shipped" && order.ShippedDate == null)
            {
                order.ShippedDate = DateTime.Now;
            }
            else if (status == "Delivered" && order.DeliveredDate == null)
            {
                order.DeliveredDate = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/Orders/PrintInvoice/5
        public async Task<IActionResult> PrintInvoice(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }
    }
}