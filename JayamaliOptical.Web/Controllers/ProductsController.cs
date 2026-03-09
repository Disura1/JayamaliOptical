using Microsoft.AspNetCore.Mvc;

namespace JayamaliOptical.Web.Controllers
{
    public class ProductsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
