using JayamaliOptical.Web.Data;
using JayamaliOptical.Web.Models;
using JayamaliOptical.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JayamaliOptical.Web.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        private readonly ApplicationDbContext _context;

        public CartController(ICartService cartService, ApplicationDbContext context)
        {
            _cartService = cartService;
            _context = context;
        }

        // GET: Cart
        public IActionResult Index()
        {
            var cart = _cartService.GetCart();
            return View(cart);
        }

        // POST: Cart/AddToCart/5
        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                return NotFound();
            }

            var cartItem = new CartItem
            {
                ProductId = product.Id,
                ProductName = product.Name,
                Price = product.Price ?? 0,
                Quantity = quantity,
                ImageUrl = product.ImageUrl1,
                CategoryName = product.Category?.Name,
                RequiresPrescription = product.RequiresPrescription
            };

            _cartService.AddToCart(cartItem);

            // Return JSON for AJAX calls
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                var cart = _cartService.GetCart();
                return Json(new
                {
                    success = true,
                    totalItems = cart.TotalItems,
                    totalPrice = cart.TotalPrice
                });
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Cart/RemoveFromCart/5
        [HttpPost]
        public IActionResult RemoveFromCart(int productId)
        {
            _cartService.RemoveFromCart(productId);
            return RedirectToAction(nameof(Index));
        }

        // POST: Cart/UpdateQuantity
        [HttpPost]
        public IActionResult UpdateQuantity(int productId, int quantity)
        {
            _cartService.UpdateQuantity(productId, quantity);
            return RedirectToAction(nameof(Index));
        }

        // POST: Cart/Clear
        [HttpPost]
        public IActionResult Clear()
        {
            _cartService.ClearCart();
            return RedirectToAction(nameof(Index));
        }

        // GET: Cart/CartSummary (for navbar)
        [HttpGet]
        public IActionResult CartSummary()
        {
            var cart = _cartService.GetCart();
            return PartialView("_CartSummary", cart);
        }
    }
}