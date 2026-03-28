using JayamaliOptical.Web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JayamaliOptical.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class MessagesController : Controller
    {
        private readonly ApplicationDbContext _context;
        public MessagesController(ApplicationDbContext context) => _context = context;

        public async Task<IActionResult> Index()
        {
            var messages = await _context.ContactMessages
                .OrderByDescending(m => m.SentAt)
                .ToListAsync();

            // Mark all as read
            var unread = messages.Where(m => !m.IsRead).ToList();
            unread.ForEach(m => m.IsRead = true);
            if (unread.Any()) await _context.SaveChangesAsync();

            ViewBag.UnreadCount = unread.Count;
            return View(messages);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var msg = await _context.ContactMessages.FindAsync(id);
            if (msg != null)
            {
                _context.ContactMessages.Remove(msg);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}