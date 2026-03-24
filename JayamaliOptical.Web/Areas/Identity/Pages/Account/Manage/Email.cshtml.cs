#nullable disable
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using JayamaliOptical.Web.Services;

namespace JayamaliOptical.Web.Areas.Identity.Pages.Account.Manage
{
    public class EmailModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IEmailService _emailService;

        public EmailModel(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            IEmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
        }

        public string Email { get; set; }
        public bool IsEmailConfirmed { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "New email")]
            public string NewEmail { get; set; }
        }

        private async Task LoadAsync(IdentityUser user)
        {
            var email = await _userManager.GetEmailAsync(user);
            Email = email;
            // Leave NewEmail empty — user should type the new address themselves
            Input = new InputModel { NewEmail = string.Empty };
            IsEmailConfirmed = await _userManager.IsEmailConfirmedAsync(user);
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();
            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostChangeEmailAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            if (!ModelState.IsValid) { await LoadAsync(user); return Page(); }

            var email = await _userManager.GetEmailAsync(user);
            if (Input.NewEmail == email)
            {
                StatusMessage = "Your email is unchanged.";
                return RedirectToPage();
            }

            var userId = await _userManager.GetUserIdAsync(user);
            var code = await _userManager.GenerateChangeEmailTokenAsync(user, Input.NewEmail);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = Url.Page(
                "/Account/ConfirmEmailChange",
                pageHandler: null,
                values: new { area = "Identity", userId, email = Input.NewEmail, code },
                protocol: Request.Scheme);

            await _emailService.SendEmailAsync(
                Input.NewEmail,
                "Confirm your email change — Jayamali Optical",
                "<p>Please confirm your new email address by clicking the link below:</p>" +
                $"<p><a href='{HtmlEncoder.Default.Encode(callbackUrl)}' " +
                "style='background:#1978bc;color:white;padding:10px 24px;border-radius:50px;" +
                "text-decoration:none;font-weight:600;'>Confirm Email Change</a></p>" +
                "<p>If you didn't request this, please ignore this email.</p>");

            StatusMessage = "Confirmation link sent — please check your new email inbox.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostSendVerificationEmailAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            // Clear ModelState errors from Input.NewEmail — it's not part of this form
            ModelState.Clear();

            var userId = await _userManager.GetUserIdAsync(user);
            var email = await _userManager.GetEmailAsync(user);
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { area = "Identity", userId, code },
                protocol: Request.Scheme);

            await _emailService.SendEmailAsync(
                email,
                "Verify your email — Jayamali Optical",
                "<p>Please verify your email address by clicking the link below:</p>" +
                $"<p><a href='{HtmlEncoder.Default.Encode(callbackUrl)}' " +
                "style='background:#1978bc;color:white;padding:10px 24px;border-radius:50px;" +
                "text-decoration:none;font-weight:600;'>Verify Email Address</a></p>");

            StatusMessage = "Verification email sent. Please check your inbox.";
            return RedirectToPage();
        }
    }
}