using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace JayamaliOptical.Web.Services
{
    public interface IEmailService
    {
        Task SendOrderConfirmationAsync(string toEmail, string customerName, string orderNumber, decimal totalAmount);
        Task SendBookingConfirmationAsync(string toEmail, string customerName, string bookingNumber, string serviceName, DateTime appointmentDate, TimeSpan appointmentTime);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendOrderConfirmationAsync(string toEmail, string customerName, string orderNumber, decimal totalAmount)
        {
            try
            {
                var emailSettings = _configuration.GetSection("EmailSettings");

                var smtpServer = emailSettings["SmtpServer"];
                var smtpPortStr = emailSettings["SmtpPort"];
                var senderEmail = emailSettings["SenderEmail"];
                var senderPassword = emailSettings["SenderPassword"];
                var enableSslStr = emailSettings["EnableSsl"];

                // Validate settings
                if (string.IsNullOrEmpty(smtpServer) || string.IsNullOrEmpty(senderEmail) || string.IsNullOrEmpty(senderPassword))
                {
                    _logger?.LogError("Email settings not configured properly");
                    return;
                }

                // Parse port and SSL safely
                if (!int.TryParse(smtpPortStr, out var smtpPort))
                {
                    smtpPort = 587; // Default port
                }

                var enableSsl = !string.IsNullOrEmpty(enableSslStr) &&
                               (enableSslStr.ToLower() == "true" || enableSslStr == "1");

                var message = new MailMessage
                {
                    From = new MailAddress(senderEmail, "Jayamali Optical"),
                    Subject = $"Order Confirmation - {orderNumber}",
                    Body = GenerateOrderEmailBody(customerName, orderNumber, totalAmount),
                    IsBodyHtml = true
                };

                message.To.Add(toEmail);

                using var smtpClient = new SmtpClient(smtpServer, smtpPort)
                {
                    Credentials = new NetworkCredential(senderEmail, senderPassword),
                    EnableSsl = enableSsl
                };

                await smtpClient.SendMailAsync(message);
                _logger?.LogInformation($"Order confirmation email sent to {toEmail}");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Failed to send order confirmation email to {toEmail}");
                throw; // Re-throw so controller knows it failed
            }
        }

        public async Task SendBookingConfirmationAsync(string toEmail, string customerName, string bookingNumber, string serviceName, DateTime appointmentDate, TimeSpan appointmentTime)
        {
            try
            {
                var emailSettings = _configuration.GetSection("EmailSettings");

                var smtpServer = emailSettings["SmtpServer"];
                var smtpPortStr = emailSettings["SmtpPort"];
                var senderEmail = emailSettings["SenderEmail"];
                var senderPassword = emailSettings["SenderPassword"];
                var enableSslStr = emailSettings["EnableSsl"];

                // Validate settings
                if (string.IsNullOrEmpty(smtpServer) || string.IsNullOrEmpty(senderEmail) || string.IsNullOrEmpty(senderPassword))
                {
                    _logger?.LogError("Email settings not configured properly");
                    return;
                }

                // Parse port and SSL safely
                if (!int.TryParse(smtpPortStr, out var smtpPort))
                {
                    smtpPort = 587; // Default port
                }

                var enableSsl = !string.IsNullOrEmpty(enableSslStr) &&
                               (enableSslStr.ToLower() == "true" || enableSslStr == "1");

                var message = new MailMessage
                {
                    From = new MailAddress(senderEmail, "Jayamali Optical"),
                    Subject = $"Appointment Booking Confirmation - {bookingNumber}",
                    Body = GenerateBookingEmailBody(customerName, bookingNumber, serviceName, appointmentDate, appointmentTime),
                    IsBodyHtml = true
                };

                message.To.Add(toEmail);

                using var smtpClient = new SmtpClient(smtpServer, smtpPort)
                {
                    Credentials = new NetworkCredential(senderEmail, senderPassword),
                    EnableSsl = enableSsl
                };

                await smtpClient.SendMailAsync(message);
                _logger?.LogInformation($"Booking confirmation email sent to {toEmail}");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Failed to send booking confirmation email to {toEmail}");
                throw; // Re-throw so controller knows it failed
            }
        }

        private string GenerateOrderEmailBody(string customerName, string orderNumber, decimal totalAmount)
        {
            return $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <h2 style='color: #0d6efd;'>Thank You for Your Order!</h2>
                    <p>Dear {customerName},</p>
                    <p>Your order has been placed successfully.</p>
                    <div style='background: #f8f9fa; padding: 20px; border-radius: 5px; margin: 20px 0;'>
                        <h3>Order Details</h3>
                        <p><strong>Order Number:</strong> {orderNumber}</p>
                        <p><strong>Total Amount:</strong> Rs. {totalAmount.ToString("0.00")}</p>
                        <p><strong>Status:</strong> Pending Confirmation</p>
                    </div>
                    <p>We will contact you shortly to confirm your order.</p>
                    <p>Thank you for choosing Jayamali Optical!</p>
                </body>
                </html>
            ";
        }

        private string GenerateBookingEmailBody(string customerName, string bookingNumber, string serviceName, DateTime appointmentDate, TimeSpan appointmentTime)
        {
            // Convert TimeSpan to readable time format
            var dateTime = DateTime.Today.Add(appointmentTime);
            var formattedTime = dateTime.ToString("hh:mm tt"); // e.g., "02:30 PM"

            return $@"
        <html>
        <body style='font-family: Arial, sans-serif;'>
            <h2 style='color: #28a745;'>Appointment Booked Successfully!</h2>
            <p>Dear {customerName},</p>
            <p>Your appointment has been scheduled.</p>
            <div style='background: #f8f9fa; padding: 20px; border-radius: 5px; margin: 20px 0;'>
                <h3>Appointment Details</h3>
                <p><strong>Booking Reference:</strong> {bookingNumber}</p>
                <p><strong>Service:</strong> {serviceName}</p>
                <p><strong>Date:</strong> {appointmentDate:dd MMMM yyyy}</p>
                <p><strong>Time:</strong> {formattedTime}</p>
            </div>
            <p>We look forward to serving you!</p>
        </body>
        </html>
    ";
        }
    }
}