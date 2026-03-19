using JayamaliOptical.Web.Data;
using JayamaliOptical.Web.Models;
using JayamaliOptical.Web.Services;
using Microsoft.AspNetCore.Identity;
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
        private readonly UserManager<IdentityUser> _userManager;

        public CheckoutController(
            ICartService cartService,
            ApplicationDbContext context,
            IEmailService emailService,
            IConfiguration configuration,
            ILogger<CheckoutController> logger,
            UserManager<IdentityUser> userManager)
        {
            _cartService = cartService;
            _context = context;
            _emailService = emailService;
            _configuration = configuration;
            _logger = logger;
            _userManager = userManager;
        }

        // GET: Checkout
        public async Task<IActionResult> Index()
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

            // Auto-fill info for logged-in users
            if (User.Identity?.IsAuthenticated == true)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    // FIX: Safe null handling for UserName
                    var userName = user.UserName;
                    if (!string.IsNullOrEmpty(userName))
                    {
                        var nameParts = userName.Split('@')[0].Split(' ');
                        model.FirstName = nameParts.FirstOrDefault()?.Capitalized() ?? string.Empty;
                    }

                    model.Email = user.Email ?? string.Empty;
                }
            }

            // Load saved profile for auto-fill (works for both guests and logged-in users)
            await LoadUserProfileAsync(model);

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
                    UserId = User.FindFirstValue(ClaimTypes.NameIdentifier)
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

                // === SAVE USER PROFILE FOR FUTURE CHECKOUTS ===
                await SaveUserProfileAsync(model, order.UserId);
                // === END SAVE PROFILE ===

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

        // Load saved profile data to auto-fill checkout form
        private async Task LoadUserProfileAsync(CheckoutViewModel model)
        {
            string? emailToCheck = null;

            // Priority 1: Use email from logged-in user
            if (User.Identity?.IsAuthenticated == true)
            {
                emailToCheck = User.FindFirstValue(ClaimTypes.Email);
            }
            // Priority 2: Use email from session (for returning guests)
            else
            {
                emailToCheck = HttpContext.Session.GetString("GuestEmail");
            }

            if (!string.IsNullOrEmpty(emailToCheck))
            {
                var profile = await _context.UserProfiles
                    .Where(p => p.Email.ToLower() == emailToCheck.ToLower())
                    .OrderByDescending(p => p.LastUsedDate)
                    .FirstOrDefaultAsync();

                if (profile != null)
                {
                    model.FirstName = profile.FirstName;
                    model.LastName = profile.LastName;
                    model.Email = profile.Email;
                    model.PhoneNumber = profile.PhoneNumber;
                    model.Address = profile.Address;
                    model.City = profile.City;
                    model.PostalCode = profile.PostalCode;
                }
            }
        }

        // Save or update user profile for faster future checkouts
        private async Task SaveUserProfileAsync(CheckoutViewModel model, string? userId)
        {
            if (string.IsNullOrEmpty(model.Email))
            {
                _logger?.LogWarning("Cannot save profile: Email is empty");
                return;
            }

            try
            {
                // Try to find existing profile by email
                var profile = await _context.UserProfiles
                    .FirstOrDefaultAsync(p => p.Email.ToLower() == model.Email.ToLower());

                if (profile != null)
                {
                    // Update existing profile
                    profile.FirstName = model.FirstName;
                    profile.LastName = model.LastName;
                    profile.PhoneNumber = model.PhoneNumber;
                    profile.Address = model.Address;
                    profile.City = model.City;
                    profile.PostalCode = model.PostalCode;
                    profile.LastUsedDate = DateTime.Now;
                    profile.UsageCount++;

                    // If user just logged in, link the profile to their account
                    if (!string.IsNullOrEmpty(userId) && string.IsNullOrEmpty(profile.UserId))
                    {
                        profile.UserId = userId;
                    }
                }
                else
                {
                    // Create new profile
                    profile = new UserProfile
                    {
                        UserId = userId,
                        Email = model.Email,
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        PhoneNumber = model.PhoneNumber,
                        Address = model.Address,
                        City = model.City,
                        PostalCode = model.PostalCode,
                        CreatedDate = DateTime.Now,
                        LastUsedDate = DateTime.Now,
                        UsageCount = 1,
                        IsDefault = true
                    };
                    _context.UserProfiles.Add(profile);
                }

                await _context.SaveChangesAsync();
                _logger?.LogInformation("User profile saved for email: {Email}", model.Email);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to save user profile for email: {Email}", model.Email);
            }
        }

        // Generate unique order number
        private string GenerateOrderNumber()
        {
            return "ORD-" + DateTime.Now.ToString("yyyyMMddHHmmss") + "-" + new Random().Next(1000, 9999);
        }
    }

    // Helper extension method
    public static class StringExtensions
    {
        public static string Capitalized(this string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            return char.ToUpper(input[0]) + input.Substring(1).ToLower();
        }
    }
}