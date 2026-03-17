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
            return View();
        }
    }
}
