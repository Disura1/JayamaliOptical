using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;

namespace JayamaliOptical.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            UserManager<IdentityUser> userManager,
            IEmailSender emailSender,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _logger = logger;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPasswordAjax([FromForm] string email)
        {
            if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            {
                return Json(new { success = false, message = "Please enter a valid email address." });
            }

            try
            {
                var user = await _userManager.FindByEmailAsync(email.Trim());

                if (user == null)
                {
                    _logger.LogInformation("ForgotPassword: no account found for {Email}", email);
                    return Json(new { success = true });
                }

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

                var resetLink = Url.PageLink(
                    pageName: "/Account/ResetPassword",
                    pageHandler: null,
                    values: new { area = "Identity", code = encodedToken },
                    protocol: Request.Scheme
                );

                if (string.IsNullOrEmpty(resetLink))
                {
                    resetLink = string.Format("{0}://{1}/Identity/Account/ResetPassword?code={2}",
                        Request.Scheme, Request.Host, encodedToken);
                }

                var emailBody = BuildResetEmailBody(resetLink);

                await _emailSender.SendEmailAsync(
                    email.Trim(),
                    "Reset Your Password - Jayamali Optical",
                    emailBody);

                _logger.LogInformation("Password reset email sent to {Email}", email);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ForgotPassword failed for {Email}", email);
                return Json(new { success = true });
            }
        }

        private static string BuildResetEmailBody(string resetLink)
        {
            var encodedLink = System.Net.WebUtility.HtmlEncode(resetLink);

            return "<!DOCTYPE html>" +
                   "<html><head><meta charset=\"utf-8\" />" +
                   "<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\" /></head>" +
                   "<body style=\"margin:0;padding:0;background:#f0f4f8;font-family:Arial,sans-serif;\">" +
                   "<div style=\"max-width:560px;margin:40px auto;background:white;border-radius:16px;" +
                   "overflow:hidden;box-shadow:0 8px 30px rgba(25,120,188,0.12);\">" +

                   "<div style=\"background:linear-gradient(135deg,#1978bc,#26a4e0);padding:32px 36px;text-align:center;\">" +
                   "<h1 style=\"margin:0;color:white;font-size:1.4rem;font-weight:800;\">Reset Your Password</h1>" +
                   "<p style=\"margin:8px 0 0;color:rgba(255,255,255,0.85);font-size:0.88rem;\">Jayamali Optical - Password Recovery</p>" +
                   "</div>" +

                   "<div style=\"padding:36px;\">" +
                   "<p style=\"margin:0 0 16px;color:#444;font-size:0.95rem;line-height:1.6;\">" +
                   "We received a request to reset the password for your Jayamali Optical account. " +
                   "Click the button below to choose a new password:</p>" +

                   "<div style=\"text-align:center;margin:28px 0;\">" +
                   "<a href=\"" + encodedLink + "\" " +
                   "style=\"display:inline-block;background:linear-gradient(135deg,#1978bc,#26a4e0);" +
                   "color:white;padding:14px 40px;border-radius:50px;text-decoration:none;" +
                   "font-weight:700;font-size:1rem;\">Reset My Password</a>" +
                   "</div>" +

                   "<p style=\"margin:0 0 12px;color:#666;font-size:0.85rem;line-height:1.6;\">" +
                   "If the button does not work, copy and paste this link into your browser:</p>" +
                   "<p style=\"word-break:break-all;background:#f8f9fa;border:1px solid #e2e8f0;" +
                   "border-radius:8px;padding:12px 14px;font-size:0.78rem;color:#555;margin:0 0 24px;\">" +
                   encodedLink + "</p>" +

                   "<div style=\"background:#fff8e1;border:1px solid #ffe082;border-radius:8px;" +
                   "padding:14px 16px;margin-bottom:16px;\">" +
                   "<p style=\"margin:0;font-size:0.83rem;color:#7a5f00;\">" +
                   "<strong>This link expires in 24 hours.</strong> " +
                   "If you did not request a password reset, you can safely ignore this email.</p>" +
                   "</div></div>" +

                   "<div style=\"background:#f8f9fa;padding:18px 36px;text-align:center;border-top:1px solid #e8eef5;\">" +
                   "<p style=\"margin:0;color:#aaa;font-size:0.75rem;\">" +
                   "Jayamali Optical - This is an automated message, please do not reply.</p>" +
                   "</div></div></body></html>";
        }
    }
}