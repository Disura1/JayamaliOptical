using JayamaliOptical.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace JayamaliOptical.Web.Controllers
{
    public class ServicesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Details()
        {
            var service = new Service
            {
                ServiceId = 1,
                ServiceName = "Eye Channeling",
                Description = "Consultation with an eye specialist to check vision problems and eye health.",
                Price = 2500,
                DurationMinutes = 30,
                IsAvailable = true,
            };
            ViewData["Service"] = service;

            return View();
        }
    }
}
