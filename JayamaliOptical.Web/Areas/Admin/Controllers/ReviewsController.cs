using JayamaliOptical.Web.Data;
using JayamaliOptical.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JayamaliOptical.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ReviewsController : Controller
    {
        private readonly ApplicationDbContext _context;
        public ReviewsController(ApplicationDbContext context) => _context = context;

        public async Task<IActionResult> Index(string filter = "All")
        {
            var reviews = _context.Reviews.AsQueryable();

            reviews = filter switch
            {
                "Pending" => reviews.Where(r => !r.IsApproved),
                "Approved" => reviews.Where(r => r.IsApproved),
                "Featured" => reviews.Where(r => r.IsFeatured),
                _ => reviews
            };

            ViewBag.Filter = filter;
            ViewBag.PendingCount = await _context.Reviews.CountAsync(r => !r.IsApproved);
            ViewBag.ApprovedCount = await _context.Reviews.CountAsync(r => r.IsApproved);
            ViewBag.FeaturedCount = await _context.Reviews.CountAsync(r => r.IsFeatured);

            return View(await reviews.OrderByDescending(r => r.SubmittedAt).ToListAsync());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var r = await _context.Reviews.FindAsync(id);
            if (r != null) { r.IsApproved = true; await _context.SaveChangesAsync(); }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var r = await _context.Reviews.FindAsync(id);
            if (r != null) { r.IsApproved = false; await _context.SaveChangesAsync(); }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleFeatured(int id)
        {
            var r = await _context.Reviews.FindAsync(id);
            if (r != null) { r.IsFeatured = !r.IsFeatured; await _context.SaveChangesAsync(); }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var r = await _context.Reviews.FindAsync(id);
            if (r != null) { _context.Reviews.Remove(r); await _context.SaveChangesAsync(); }
            TempData["Success"] = "Review deleted.";
            return RedirectToAction(nameof(Index));
        }
    }
}