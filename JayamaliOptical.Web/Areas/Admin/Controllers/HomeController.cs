using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using JayamaliOptical.Web.Models;
using JayamaliOptical.Web.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

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
            var model = new DashboardViewModel
            {
                TotalCustomers = await _context.Users
                    .Where(u => !u.Email!.EndsWith("@jayamalioptical.com"))
                    .CountAsync(),

                TotalOrders = 0,
                TotalProducts = 0,
                TodayAppointments = 0,
                TotalRevenue = 0
            };

            return View(model);
        }
    }
}