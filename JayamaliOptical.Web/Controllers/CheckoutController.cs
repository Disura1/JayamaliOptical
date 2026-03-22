using JayamaliOptical.Web.Data;
using JayamaliOptical.Web.Models;
using JayamaliOptical.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        private readonly IWebHostEnvironment _environment;

        public CheckoutController(
            ICartService cartService,
            ApplicationDbContext context,
            IEmailService emailService,
            IConfiguration configuration,
            ILogger<CheckoutController> logger,
            UserManager<IdentityUser> userManager,
            IWebHostEnvironment environment)
        {
            _cartService = cartService;
            _context = context;
            _emailService = emailService;
            _configuration = configuration;
            _logger = logger;
            _userManager = userManager;
            _environment = environment;
        }

        // GET: Checkout
        public async Task<IActionResult> Index()
        {
            var cart = _cartService.GetCart();
            if (cart.Items.Count == 0)
                return RedirectToAction("Index", "Cart");

            var model = new CheckoutViewModel { Cart = cart };

            model.CartRequiresPrescription = cart.Items?.Any(i => i.RequiresPrescription) ?? false;

            if (model.CartRequiresPrescription && User.Identity?.IsAuthenticated == true)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!string.IsNullOrEmpty(userId))
                {
                    model.UserPrescriptions = await _context.Prescriptions
                        .Where(p => p.UserId == userId)
                        .OrderByDescending(p => p.IsDefault)
                        .ThenByDescending(p => p.CreatedDate)
                        .ToListAsync();

                    if (model.UserPrescriptions.Count > 0)
                    {
                        var defaultRx = model.UserPrescriptions.FirstOrDefault(p => p.IsDefault);
                        model.SelectedPrescriptionId = defaultRx?.Id ?? model.UserPrescriptions[0].Id;
                    }
                }
            }

            if (User.Identity?.IsAuthenticated == true)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    model.FirstName = user.UserName?.Split(' ')[0] ?? string.Empty;
                    model.Email = user.Email ?? string.Empty;
                }
            }

            return View(model);
        }

        // POST: Checkout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(CheckoutViewModel model)
        {
            var cart = _cartService.GetCart();
            if (cart.Items.Count == 0)
                return RedirectToAction("Index", "Cart");

            bool cartRequiresPrescription = cart.Items?.Any(i => i.RequiresPrescription) ?? false;

            if (cartRequiresPrescription)
            {
                bool hasPrescription = false;

                if (model.SelectedPrescriptionId.HasValue && User.Identity?.IsAuthenticated == true)
                {
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    var prescription = await _context.Prescriptions
                        .FirstOrDefaultAsync(p => p.Id == model.SelectedPrescriptionId.Value && p.UserId == userId);

                    if (prescription != null)
                    {
                        hasPrescription = true;
                        prescription.LastUsedDate = DateTime.Now;
                        await _context.SaveChangesAsync();
                    }
                }

                if (model.UploadPrescriptionFile != null && model.UploadPrescriptionFile.Length > 0)
                    hasPrescription = true;

                if (!hasPrescription)
                {
                    ModelState.AddModelError("", "⚠️ Prescription Required: Please select a saved prescription OR upload a new one.");

                    if (User.Identity?.IsAuthenticated == true)
                    {
                        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                        model.UserPrescriptions = await _context.Prescriptions
                            .Where(p => p.UserId == userId)
                            .OrderByDescending(p => p.IsDefault)
                            .ToListAsync();
                    }

                    model.Cart = cart;
                    model.CartRequiresPrescription = true;
                    return View(model);
                }
            }

            if (ModelState.IsValid)
            {
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

                // Save prescription — Option 1: saved prescription
                if (cartRequiresPrescription && model.SelectedPrescriptionId.HasValue)
                {
                    order.PrescriptionId = model.SelectedPrescriptionId.Value;
                }

                // Save prescription — Option 2: uploaded file
                if (cartRequiresPrescription && model.UploadPrescriptionFile != null && model.UploadPrescriptionFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "prescriptions");
                    Directory.CreateDirectory(uploadsFolder);

                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + model.UploadPrescriptionFile.FileName;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.UploadPrescriptionFile!.CopyToAsync(fileStream);
                    }

                    order.PrescriptionImagePath = "/uploads/prescriptions/" + uniqueFileName;
                    order.PrescriptionFileName = model.UploadPrescriptionFile.FileName;
                }

                foreach (var item in cart?.Items ?? new List<CartItem>())
                {
                    order.OrderItems.Add(new OrderItem
                    {
                        ProductId = item.ProductId,
                        ProductName = item.ProductName,
                        Price = item.Price,
                        Quantity = item.Quantity
                    });
                }

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                await SaveUserProfileAsync(model, order.UserId);

                try
                {
                    await _emailService.SendOrderConfirmationAsync(
                        model.Email,
                        $"{model.FirstName} {model.LastName}",
                        order.OrderNumber,
                        order.TotalAmount);
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Email sending failed");
                }

                _cartService.ClearCart();
                return RedirectToAction("Confirmation", new { orderId = order.Id });
            }

            model.Cart = cart;
            return View(model);
        }

        // GET: Checkout/Confirmation/5
        public async Task<IActionResult> Confirmation(int? orderId)
        {
            if (orderId == null) return NotFound();

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null) return NotFound();

            return View(order);
        }

        private async Task SaveUserProfileAsync(CheckoutViewModel model, string? userId)
        {
            if (string.IsNullOrEmpty(model.Email)) return;

            try
            {
                var profile = await _context.UserProfiles
                    .FirstOrDefaultAsync(p => p.Email.ToLower() == model.Email.ToLower());

                if (profile != null)
                {
                    profile.FirstName = model.FirstName;
                    profile.LastName = model.LastName;
                    profile.PhoneNumber = model.PhoneNumber;
                    profile.Address = model.Address;
                    profile.City = model.City;
                    profile.PostalCode = model.PostalCode;
                    profile.LastUsedDate = DateTime.Now;
                    profile.UsageCount++;
                    if (!string.IsNullOrEmpty(userId) && string.IsNullOrEmpty(profile.UserId))
                        profile.UserId = userId;
                }
                else
                {
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
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to save user profile for email: {Email}", model.Email);
            }
        }

        private static string GenerateOrderNumber()
        {
            return "ORD-" + DateTime.Now.ToString("yyyyMMddHHmmss") + "-" + new Random().Next(1000, 9999);
        }
    }

    public static class StringExtensions
    {
        public static string Capitalized(this string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            return char.ToUpper(input[0]) + input.Substring(1).ToLower();
        }
    }
}