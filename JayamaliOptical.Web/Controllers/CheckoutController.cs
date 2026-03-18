using JayamaliOptical.Web.Data;
using JayamaliOptical.Web.Models;
using JayamaliOptical.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace JayamaliOptical.Web.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly ICartService _cartService;
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<CheckoutController> _logger;

        public CheckoutController(
            ICartService cartService,
            ApplicationDbContext context,
            IEmailService emailService,
            IConfiguration configuration,
            ILogger<CheckoutController> logger)
        {
            _cartService = cartService;
            _context = context;
            _emailService = emailService;
            _configuration = configuration;
            _logger = logger; 
        }

        // GET: Checkout
        public IActionResult Index()
        {
            var cart = _cartService.GetCart();

            if (!cart.Items.Any())
            {
                return RedirectToAction("Index", "Cart");
            }

            var model = new CheckoutViewModel
            {
                Cart = cart
            };

            return View(model);
        }

        // POST: Checkout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(CheckoutViewModel model)
        {
            var cart = _cartService.GetCart();

            if (!cart.Items.Any())
            {
                return RedirectToAction("Index", "Cart");
            }

            if (ModelState.IsValid)
            {
                // Create order
                var order = new Order
                {
                    OrderNumber = GenerateOrderNumber(),
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    Address = model.Address,
                    City = model.City,
                    PostalCode = model.PostalCode,
                    OrderNotes = model.OrderNotes,
                    TotalAmount = cart.TotalPrice,
                    OrderDate = DateTime.Now,
                    Status = "Pending",
                    UserId = User.FindFirstValue(ClaimTypes.NameIdentifier)  // ← ADD THIS
                };

                // Add order items
                foreach (var item in cart.Items)
                {
                    order.OrderItems.Add(new OrderItem
                    {
                        ProductId = item.ProductId,
                        ProductName = item.ProductName,
                        Price = item.Price,
                        Quantity = item.Quantity
                    });
                }

                // Save to database
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                // Send confirmation email
                try
                {
                    await _emailService.SendOrderConfirmationAsync(
                        model.Email,
                        $"{model.FirstName} {model.LastName}",
                        order.OrderNumber,
                        order.TotalAmount
                    );
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Email sending failed");
                }

                // Clear cart
                _cartService.ClearCart();

                // Redirect to confirmation
                return RedirectToAction("Confirmation", new { orderId = order.Id });
            }

            model.Cart = cart;
            return View(model);
        }

        // GET: Checkout/Confirmation/5
        public async Task<IActionResult> Confirmation(int? orderId)
        {
            if (orderId == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // Generate unique order number
        private string GenerateOrderNumber()
        {
            return "ORD-" + DateTime.Now.ToString("yyyyMMddHHmmss") + "-" + new Random().Next(1000, 9999);
        }


    }
}