using Microsoft.AspNetCore.Identity.UI.Services;
using JayamaliOptical.Web.Services;

namespace JayamaliOptical.Web.Services
{
    public class IdentityEmailSender : IEmailSender
    {
        private readonly IEmailService _emailService;

        public IdentityEmailSender(IEmailService emailService)
        {
            _emailService = emailService;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            await _emailService.SendEmailAsync(email, subject, htmlMessage);
        }
    }
}