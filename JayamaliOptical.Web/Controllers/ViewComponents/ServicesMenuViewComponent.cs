using JayamaliOptical.Web.Data;
using JayamaliOptical.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JayamaliOptical.Web.ViewComponents
{
    public class ServicesMenuViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public ServicesMenuViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var services = await _context.Services
                .Where(s => s.IsActive)
                .OrderBy(s => s.DisplayOrder)
                .Take(10) // Limit to 10 services in dropdown
                .ToListAsync();

            return View(services);
        }
    }
}