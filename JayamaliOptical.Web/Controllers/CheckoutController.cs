using JayamaliOptical.Web.Data;
using JayamaliOptical.Web.Models;
using JayamaliOptical.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

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

        // ── GET: /Checkout ─────────────────────────────────────────────────
        public async Task<IActionResult> Index()
        {
            var cart = _cartService.GetCart();
            if (cart.Items.Count == 0)
                return RedirectToAction("Index", "Cart");

            var model = new CheckoutViewModel { Cart = cart };
            model.CartRequiresPrescription = cart.Items?.Any(i => i.RequiresPrescription) ?? false;

            // Load payment settings
            var settings = await _context.SiteSettings.FirstOrDefaultAsync();
            model.CodEnabled = settings?.CodEnabled ?? true;
            model.PayHereEnabled = settings?.PayHereEnabled ?? false;
            model.PaymentMethod = model.CodEnabled ? "COD" : (model.PayHereEnabled ? "PayHere" : "COD");

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

        // ── POST: /Checkout ────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(CheckoutViewModel model)
        {
            var cart = _cartService.GetCart();
            if (cart.Items.Count == 0)
                return RedirectToAction("Index", "Cart");

            // Re-load payment settings and re-inject into model
            var settings = await _context.SiteSettings.FirstOrDefaultAsync();
            model.CodEnabled = settings?.CodEnabled ?? true;
            model.PayHereEnabled = settings?.PayHereEnabled ?? false;

            // Validate payment method is enabled
            if (model.PaymentMethod == "COD" && !model.CodEnabled)
                ModelState.AddModelError("PaymentMethod", "Cash on Delivery is currently unavailable.");
            else if (model.PaymentMethod == "PayHere" && !model.PayHereEnabled)
                ModelState.AddModelError("PaymentMethod", "Online payment is currently unavailable.");
            else if (model.PaymentMethod != "COD" && model.PaymentMethod != "PayHere")
                ModelState.AddModelError("PaymentMethod", "Please select a valid payment method.");

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
                    PaymentMethod = model.PaymentMethod,
                    PaymentStatus = model.PaymentMethod == "COD" ? "Pending" : "Awaiting",
                    UserId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                };

                // Prescription — option 1: saved
                if (cartRequiresPrescription && model.SelectedPrescriptionId.HasValue)
                    order.PrescriptionId = model.SelectedPrescriptionId.Value;

                // Prescription — option 2: uploaded file
                if (cartRequiresPrescription && model.UploadPrescriptionFile != null && model.UploadPrescriptionFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "prescriptions");
                    Directory.CreateDirectory(uploadsFolder);
                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + model.UploadPrescriptionFile.FileName;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                        await model.UploadPrescriptionFile.CopyToAsync(fileStream);

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

                // ── PayHere: redirect to payment gateway ──────────────────
                if (model.PaymentMethod == "PayHere" && settings != null)
                {
                    _cartService.ClearCart();
                    return BuildPayHereRedirect(order, settings);
                }

                // ── COD: send emails and show confirmation ─────────────────
                try
                {
                    await _emailService.SendOrderConfirmationAsync(
                        model.Email, $"{model.FirstName} {model.LastName}",
                        order.OrderNumber, order.TotalAmount);
                }
                catch (Exception ex) { _logger?.LogError(ex, "Confirmation email failed"); }

                try
                {
                    await _emailService.SendNewOrderAlertAsync(
                        order.OrderNumber, $"{model.FirstName} {model.LastName}",
                        model.Email, order.TotalAmount);
                }
                catch { /* never block checkout */ }

                _cartService.ClearCart();
                return RedirectToAction("Confirmation", new { orderId = order.Id });
            }

            model.Cart = cart;
            return View(model);
        }

        // ── PayHere: build auto-submit HTML redirect page ──────────────────
        private IActionResult BuildPayHereRedirect(Order order, SiteSettings settings)
        {
            var baseUrl = settings.PayHereSandbox
                ? "https://sandbox.payhere.lk/pay/checkout"
                : "https://www.payhere.lk/pay/checkout";
            var returnUrl = $"{Request.Scheme}://{Request.Host}/Checkout/PayHereReturn";
            var cancelUrl = $"{Request.Scheme}://{Request.Host}/Checkout/PayHereCancel";
            var notifyUrl = $"{Request.Scheme}://{Request.Host}/Checkout/PayHereNotify";
            var amount = order.TotalAmount.ToString("F2");

            // Hash = MD5(merchant_id + order_id + amount + currency + MD5(merchant_secret).ToUpper())
            var secretHash = Md5(settings.PayHereMerchantSecret.ToUpper());
            var hash = Md5($"{settings.PayHereMerchantId}{order.OrderNumber}{amount}LKR{secretHash}").ToUpper();

            var html = $@"<!DOCTYPE html>
<html>
<head><title>Redirecting to PayHere...</title></head>
<body onload=""document.getElementById('ph').submit()"" style=""font-family:sans-serif;text-align:center;padding-top:80px;"">
    <p>Redirecting to PayHere secure payment gateway...</p>
    <form id=""ph"" method=""post"" action=""{baseUrl}"">
        <input type=""hidden"" name=""merchant_id""  value=""{settings.PayHereMerchantId}"" />
        <input type=""hidden"" name=""return_url""   value=""{returnUrl}"" />
        <input type=""hidden"" name=""cancel_url""   value=""{cancelUrl}"" />
        <input type=""hidden"" name=""notify_url""   value=""{notifyUrl}"" />
        <input type=""hidden"" name=""order_id""     value=""{order.OrderNumber}"" />
        <input type=""hidden"" name=""items""        value=""Jayamali Optical Order {order.OrderNumber}"" />
        <input type=""hidden"" name=""currency""     value=""LKR"" />
        <input type=""hidden"" name=""amount""       value=""{amount}"" />
        <input type=""hidden"" name=""first_name""   value=""{order.FirstName}"" />
        <input type=""hidden"" name=""last_name""    value=""{order.LastName}"" />
        <input type=""hidden"" name=""email""        value=""{order.Email}"" />
        <input type=""hidden"" name=""phone""        value=""{order.PhoneNumber}"" />
        <input type=""hidden"" name=""address""      value=""{order.Address}"" />
        <input type=""hidden"" name=""city""         value=""{order.City}"" />
        <input type=""hidden"" name=""country""      value=""Sri Lanka"" />
        <input type=""hidden"" name=""hash""         value=""{hash}"" />
    </form>
</body>
</html>";
            return Content(html, "text/html");
        }

        // ── PayHere: return URL (user comes back after payment) ────────────
        public async Task<IActionResult> PayHereReturn(string order_id, string status_code)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderNumber == order_id);
            if (order != null)
            {
                if (status_code == "2") // 2 = Success
                {
                    order.PaymentStatus = "Paid";
                    order.Status = "Processing";
                    await _context.SaveChangesAsync();
                }
            }
            TempData["PayHereStatus"] = status_code == "2" ? "success" : "pending";
            return RedirectToAction("Confirmation", new { orderId = order?.Id });
        }

        // ── PayHere: cancel URL ────────────────────────────────────────────
        public IActionResult PayHereCancel()
        {
            TempData["PayHereCancelled"] = true;
            return RedirectToAction("Index", "Cart");
        }

        // ── PayHere: IPN (server-to-server notification) ───────────────────
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> PayHereNotify(
            string merchant_id, string order_id, string payhere_amount,
            string payhere_currency, string status_code, string md5sig)
        {
            var settings = await _context.SiteSettings.FirstOrDefaultAsync();
            if (settings == null) return Ok();

            // Verify signature
            var secretHash = Md5(settings.PayHereMerchantSecret.ToUpper());
            var expected = Md5($"{merchant_id}{order_id}{payhere_amount}{payhere_currency}{status_code}{secretHash}").ToUpper();

            if (expected != md5sig?.ToUpper())
            {
                _logger.LogWarning("PayHere IPN hash mismatch for order {OrderId}", order_id);
                return BadRequest("Hash mismatch");
            }

            var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderNumber == order_id);
            if (order != null)
            {
                order.PaymentStatus = status_code == "2" ? "Paid"
                                       : status_code == "0" ? "Pending"
                                       : "Failed";
                order.PaymentReference = $"PH-{order_id}";
                if (status_code == "2") order.Status = "Processing";
                await _context.SaveChangesAsync();
            }

            return Ok();
        }

        // ── GET: /Checkout/Confirmation/5 ──────────────────────────────────
        public async Task<IActionResult> Confirmation(int? orderId)
        {
            if (orderId == null) return NotFound();

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null) return NotFound();
            return View(order);
        }

        // ── Helpers ────────────────────────────────────────────────────────
        private static string Md5(string input)
        {
            using var md5 = MD5.Create();
            var bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
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
                    profile.FirstName = model.FirstName; profile.LastName = model.LastName;
                    profile.PhoneNumber = model.PhoneNumber; profile.Address = model.Address;
                    profile.City = model.City; profile.PostalCode = model.PostalCode;
                    profile.LastUsedDate = DateTime.Now; profile.UsageCount++;
                    if (!string.IsNullOrEmpty(userId) && string.IsNullOrEmpty(profile.UserId))
                        profile.UserId = userId;
                }
                else
                {
                    _context.UserProfiles.Add(new UserProfile
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
                    });
                }
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to save user profile for {Email}", model.Email);
            }
        }

        private static string GenerateOrderNumber()
            => "ORD-" + DateTime.Now.ToString("yyyyMMddHHmmss") + "-" + new Random().Next(1000, 9999);
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