using System.Net;
using System.Net.Mail;

namespace JayamaliOptical.Web.Services
{
    public interface IEmailService
    {
        Task SendOrderConfirmationAsync(string toEmail, string customerName, string orderNumber, decimal totalAmount);
        Task SendBookingConfirmationAsync(string toEmail, string customerName, string bookingNumber, string serviceName, DateTime appointmentDate, TimeSpan appointmentTime);
        Task SendContactMessageAsync(string senderName, string senderEmail, string phone, string subject, string message);
        Task SendEmailAsync(string toEmail, string subject, string htmlBody);
        Task SendNewOrderAlertAsync(string orderNumber, string customerName, string customerEmail, decimal totalAmount);
        Task SendNewBookingAlertAsync(string bookingNumber, string customerName, string customerEmail, string serviceName, DateTime appointmentDate, TimeSpan appointmentTime);
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

        // ── Shared: build SmtpClient from config ──────────────────────────
        private (SmtpClient client, string senderEmail) BuildSmtp()
        {
            var s = _configuration.GetSection("EmailSettings");
            var smtpServer = s["SmtpServer"] ?? "smtp.gmail.com";
            var senderEmail = s["SenderEmail"] ?? "";
            var senderPassword = s["SenderPassword"] ?? "";
            var enableSsl = (s["EnableSsl"] ?? "true").ToLower() == "true";

            if (!int.TryParse(s["SmtpPort"], out var port)) port = 587;

            var client = new SmtpClient(smtpServer, port)
            {
                Credentials = new NetworkCredential(senderEmail, senderPassword),
                EnableSsl = enableSsl
            };
            return (client, senderEmail);
        }

        // ── Order Confirmation ────────────────────────────────────────────
        public async Task SendOrderConfirmationAsync(string toEmail, string customerName, string orderNumber, decimal totalAmount)
        {
            try
            {
                var (smtp, from) = BuildSmtp();
                var mail = new MailMessage
                {
                    From = new MailAddress(from, "Jayamali Optical"),
                    Subject = $"Order Confirmation - {orderNumber}",
                    Body = OrderEmailBody(customerName, orderNumber, totalAmount),
                    IsBodyHtml = true
                };
                mail.To.Add(toEmail);
                using (smtp) await smtp.SendMailAsync(mail);
                _logger?.LogInformation("Order confirmation sent to {Email}", toEmail);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to send order confirmation to {Email}", toEmail);
                throw;
            }
        }

        // ── Booking Confirmation ──────────────────────────────────────────
        public async Task SendBookingConfirmationAsync(string toEmail, string customerName, string bookingNumber, string serviceName, DateTime appointmentDate, TimeSpan appointmentTime)
        {
            try
            {
                var (smtp, from) = BuildSmtp();
                var mail = new MailMessage
                {
                    From = new MailAddress(from, "Jayamali Optical"),
                    Subject = $"Appointment Booking Confirmation - {bookingNumber}",
                    Body = BookingEmailBody(customerName, bookingNumber, serviceName, appointmentDate, appointmentTime),
                    IsBodyHtml = true
                };
                mail.To.Add(toEmail);
                using (smtp) await smtp.SendMailAsync(mail);
                _logger?.LogInformation("Booking confirmation sent to {Email}", toEmail);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to send booking confirmation to {Email}", toEmail);
                throw;
            }
        }

        // ── Contact Message Notification ──────────────────────────────────
        public async Task SendContactMessageAsync(string senderName, string senderEmail, string phone, string subject, string message)
        {
            try
            {
                var (smtp, adminEmail) = BuildSmtp();

                // Get the notification recipient from config (TestNotifyEmail if set, else SenderEmail)
                var notifyEmail = _configuration["EmailSettings:TestNotifyEmail"];
                if (string.IsNullOrEmpty(notifyEmail)) notifyEmail = adminEmail;

                var subjectLine = string.IsNullOrEmpty(subject) ? "General Inquiry" : subject;
                var phoneLine = string.IsNullOrEmpty(phone) ? "Not provided" : phone;

                var body = ContactEmailBody(senderName, senderEmail, phoneLine, subjectLine, message);

                var mail = new MailMessage
                {
                    From = new MailAddress(adminEmail, "Jayamali Optical Website"),
                    Subject = $"[Website Enquiry] {subjectLine} — from {senderName}",
                    Body = body,
                    IsBodyHtml = true
                };

                mail.To.Add(notifyEmail);
                mail.ReplyToList.Add(new MailAddress(senderEmail, senderName));

                using (smtp) await smtp.SendMailAsync(mail);
                _logger?.LogInformation("Contact message from {Email} forwarded to {Notify}", senderEmail, notifyEmail);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to send contact message notification");
                // Don't rethrow — message already saved to DB
            }
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            try
            {
                var (smtp, from) = BuildSmtp();
                var mail = new MailMessage
                {
                    From = new MailAddress(from, "Jayamali Optical"),
                    Subject = subject,
                    Body = htmlBody,
                    IsBodyHtml = true
                };
                mail.To.Add(toEmail);
                using (smtp) await smtp.SendMailAsync(mail);
                _logger?.LogInformation("Email sent to {Email}", toEmail);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to send email to {Email}", toEmail);
                throw;
            }
        }

        // ── New Order Admin Alert ─────────────────────────────────────────
        public async Task SendNewOrderAlertAsync(string orderNumber, string customerName,
            string customerEmail, decimal totalAmount)
        {
            try
            {
                var (smtp, from) = BuildSmtp();
                var notifyEmail = _configuration["EmailSettings:TestNotifyEmail"];
                if (string.IsNullOrEmpty(notifyEmail)) notifyEmail = from;

                var mail = new MailMessage
                {
                    From = new MailAddress(from, "Jayamali Optical"),
                    Subject = $"🛒 New Order Received — {orderNumber}",
                    Body = NewOrderAlertBody(orderNumber, customerName, customerEmail, totalAmount),
                    IsBodyHtml = true
                };
                mail.To.Add(notifyEmail);
                using (smtp) await smtp.SendMailAsync(mail);
                _logger?.LogInformation("New order alert sent for {OrderNumber}", orderNumber);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to send new order alert");
                // Don't rethrow — order is already saved
            }
        }

        // ── New Booking Admin Alert ───────────────────────────────────────
        public async Task SendNewBookingAlertAsync(string bookingNumber, string customerName,
            string customerEmail, string serviceName,
            DateTime appointmentDate, TimeSpan appointmentTime)
        {
            try
            {
                var (smtp, from) = BuildSmtp();
                var notifyEmail = _configuration["EmailSettings:TestNotifyEmail"];
                if (string.IsNullOrEmpty(notifyEmail)) notifyEmail = from;

                var mail = new MailMessage
                {
                    From = new MailAddress(from, "Jayamali Optical"),
                    Subject = $"📅 New Appointment — {bookingNumber}",
                    Body = NewBookingAlertBody(bookingNumber, customerName, customerEmail,
                                     serviceName, appointmentDate, appointmentTime),
                    IsBodyHtml = true
                };
                mail.To.Add(notifyEmail);
                using (smtp) await smtp.SendMailAsync(mail);
                _logger?.LogInformation("New booking alert sent for {BookingNumber}", bookingNumber);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to send new booking alert");
            }
        }

        // ── Email Bodies ──────────────────────────────────────────────────

        private static string NewOrderAlertBody(string orderNumber, string customerName,
            string customerEmail, decimal totalAmount)
        {
            return "<html><body style='font-family:Arial,sans-serif;color:#333;'>"
                 + "<div style='max-width:580px;margin:0 auto;border:1px solid #e0e0e0;border-radius:10px;overflow:hidden;'>"
                 + "<div style='background:linear-gradient(135deg,#1978bc,#26a4e0);padding:22px 28px;color:white;'>"
                 + "<h2 style='margin:0;font-size:1.3rem;'>🛒 New Order Received</h2>"
                 + "<p style='margin:4px 0 0;opacity:0.85;font-size:0.9rem;'>Action required — review in admin panel</p>"
                 + "</div>"
                 + "<div style='padding:28px;'>"
                 + "<table style='width:100%;border-collapse:collapse;'>"
                 + "<tr><td style='padding:8px 0;color:#666;width:130px;'><strong>Order No.</strong></td><td style='padding:8px 0;font-weight:700;color:#1978bc;'>" + orderNumber + "</td></tr>"
                 + "<tr><td style='padding:8px 0;color:#666;'><strong>Customer</strong></td><td style='padding:8px 0;'>" + customerName + "</td></tr>"
                 + "<tr><td style='padding:8px 0;color:#666;'><strong>Email</strong></td><td style='padding:8px 0;'>" + customerEmail + "</td></tr>"
                 + "<tr><td style='padding:8px 0;color:#666;'><strong>Amount</strong></td><td style='padding:8px 0;font-weight:700;font-size:1.1rem;'>Rs. " + totalAmount.ToString("0.00") + "</td></tr>"
                 + "<tr><td style='padding:8px 0;color:#666;'><strong>Time</strong></td><td style='padding:8px 0;'>" + DateTime.Now.ToString("dd MMM yyyy, hh:mm tt") + "</td></tr>"
                 + "</table>"
                 + "<div style='margin-top:24px;text-align:center;'>"
                 + "<a href='/Admin/Orders' style='display:inline-block;background:#1978bc;color:white;padding:12px 32px;border-radius:50px;text-decoration:none;font-weight:700;'>View Order in Admin Panel</a>"
                 + "</div>"
                 + "</div>"
                 + "<div style='background:#f8f9fa;padding:14px 28px;font-size:0.8rem;color:#999;text-align:center;'>"
                 + "Jayamali Optical — Automated Notification System"
                 + "</div>"
                 + "</div></body></html>";
        }

        private static string NewBookingAlertBody(string bookingNumber, string customerName,
            string customerEmail, string serviceName,
            DateTime appointmentDate, TimeSpan appointmentTime)
        {
            var formattedTime = DateTime.Today.Add(appointmentTime).ToString("hh:mm tt");
            return "<html><body style='font-family:Arial,sans-serif;color:#333;'>"
                 + "<div style='max-width:580px;margin:0 auto;border:1px solid #e0e0e0;border-radius:10px;overflow:hidden;'>"
                 + "<div style='background:linear-gradient(135deg,#28a745,#20c997);padding:22px 28px;color:white;'>"
                 + "<h2 style='margin:0;font-size:1.3rem;'>📅 New Appointment Booked</h2>"
                 + "<p style='margin:4px 0 0;opacity:0.85;font-size:0.9rem;'>Review and confirm in admin panel</p>"
                 + "</div>"
                 + "<div style='padding:28px;'>"
                 + "<table style='width:100%;border-collapse:collapse;'>"
                 + "<tr><td style='padding:8px 0;color:#666;width:130px;'><strong>Booking Ref.</strong></td><td style='padding:8px 0;font-weight:700;color:#28a745;'>" + bookingNumber + "</td></tr>"
                 + "<tr><td style='padding:8px 0;color:#666;'><strong>Customer</strong></td><td style='padding:8px 0;'>" + customerName + "</td></tr>"
                 + "<tr><td style='padding:8px 0;color:#666;'><strong>Email</strong></td><td style='padding:8px 0;'>" + customerEmail + "</td></tr>"
                 + "<tr><td style='padding:8px 0;color:#666;'><strong>Service</strong></td><td style='padding:8px 0;font-weight:600;'>" + serviceName + "</td></tr>"
                 + "<tr><td style='padding:8px 0;color:#666;'><strong>Date</strong></td><td style='padding:8px 0;font-weight:600;'>" + appointmentDate.ToString("dd MMMM yyyy") + "</td></tr>"
                 + "<tr><td style='padding:8px 0;color:#666;'><strong>Time</strong></td><td style='padding:8px 0;font-weight:600;'>" + formattedTime + "</td></tr>"
                 + "</table>"
                 + "<div style='margin-top:24px;text-align:center;'>"
                 + "<a href='/Admin/Bookings' style='display:inline-block;background:#28a745;color:white;padding:12px 32px;border-radius:50px;text-decoration:none;font-weight:700;'>View Booking in Admin Panel</a>"
                 + "</div>"
                 + "</div>"
                 + "<div style='background:#f8f9fa;padding:14px 28px;font-size:0.8rem;color:#999;text-align:center;'>"
                 + "Jayamali Optical — Automated Notification System"
                 + "</div>"
                 + "</div></body></html>";
        }

        private static string OrderEmailBody(string customerName, string orderNumber, decimal totalAmount)
        {
            return "<html><body style='font-family:Arial,sans-serif;'>"
                 + "<h2 style='color:#0d6efd;'>Thank You for Your Order!</h2>"
                 + "<p>Dear " + customerName + ",</p>"
                 + "<p>Your order has been placed successfully.</p>"
                 + "<div style='background:#f8f9fa;padding:20px;border-radius:5px;margin:20px 0;'>"
                 + "<h3>Order Details</h3>"
                 + "<p><strong>Order Number:</strong> " + orderNumber + "</p>"
                 + "<p><strong>Total Amount:</strong> Rs. " + totalAmount.ToString("0.00") + "</p>"
                 + "<p><strong>Status:</strong> Pending Confirmation</p>"
                 + "</div>"
                 + "<p>We will contact you shortly to confirm your order.</p>"
                 + "<p>Thank you for choosing Jayamali Optical!</p>"
                 + "</body></html>";
        }

        private static string BookingEmailBody(string customerName, string bookingNumber, string serviceName, DateTime appointmentDate, TimeSpan appointmentTime)
        {
            var formattedTime = DateTime.Today.Add(appointmentTime).ToString("hh:mm tt");
            return "<html><body style='font-family:Arial,sans-serif;'>"
                 + "<h2 style='color:#28a745;'>Appointment Booked Successfully!</h2>"
                 + "<p>Dear " + customerName + ",</p>"
                 + "<p>Your appointment has been scheduled.</p>"
                 + "<div style='background:#f8f9fa;padding:20px;border-radius:5px;margin:20px 0;'>"
                 + "<h3>Appointment Details</h3>"
                 + "<p><strong>Booking Reference:</strong> " + bookingNumber + "</p>"
                 + "<p><strong>Service:</strong> " + serviceName + "</p>"
                 + "<p><strong>Date:</strong> " + appointmentDate.ToString("dd MMMM yyyy") + "</p>"
                 + "<p><strong>Time:</strong> " + formattedTime + "</p>"
                 + "</div>"
                 + "<p>We look forward to serving you!</p>"
                 + "</body></html>";
        }

        private static string ContactEmailBody(string name, string email, string phone, string subject, string message)
        {
            var escaped = message.Replace("\n", "<br/>");
            return "<html><body style='font-family:Arial,sans-serif;color:#333;'>"
                 + "<div style='max-width:600px;margin:0 auto;border:1px solid #e0e0e0;border-radius:8px;overflow:hidden;'>"
                 + "<div style='background:linear-gradient(135deg,#1978bc,#26a4e0);padding:24px;color:white;'>"
                 + "<h2 style='margin:0;'>New Website Enquiry</h2>"
                 + "<p style='margin:4px 0 0;opacity:0.85;'>Received from the Contact Us form</p>"
                 + "</div>"
                 + "<div style='padding:28px;'>"
                 + "<table style='width:100%;border-collapse:collapse;'>"
                 + "<tr><td style='padding:8px 0;color:#666;width:100px;'><strong>Name</strong></td><td style='padding:8px 0;'>" + name + "</td></tr>"
                 + "<tr><td style='padding:8px 0;color:#666;'><strong>Email</strong></td><td style='padding:8px 0;'><a href='mailto:" + email + "'>" + email + "</a></td></tr>"
                 + "<tr><td style='padding:8px 0;color:#666;'><strong>Phone</strong></td><td style='padding:8px 0;'>" + phone + "</td></tr>"
                 + "<tr><td style='padding:8px 0;color:#666;'><strong>Subject</strong></td><td style='padding:8px 0;'>" + subject + "</td></tr>"
                 + "</table>"
                 + "<hr style='border:none;border-top:1px solid #eee;margin:16px 0;'/>"
                 + "<p style='color:#666;margin-bottom:8px;'><strong>Message:</strong></p>"
                 + "<div style='background:#f8f9fa;border-left:4px solid #1978bc;padding:16px;border-radius:4px;line-height:1.6;'>"
                 + escaped
                 + "</div>"
                 + "<hr style='border:none;border-top:1px solid #eee;margin:20px 0;'/>"
                 + "<p style='color:#888;font-size:0.85rem;margin:0;'>Sent on " + DateTime.Now.ToString("dd MMM yyyy, hh:mm tt") + " via Jayamali Optical website</p>"
                 + "</div>"
                 + "<div style='background:#f8f9fa;padding:16px;text-align:center;'>"
                 + "<a href='mailto:" + email + "?subject=Re: " + subject + "' style='display:inline-block;background:#1978bc;color:white;padding:10px 28px;border-radius:50px;text-decoration:none;font-weight:600;'>Reply to " + name + "</a>"
                 + "</div>"
                 + "</div>"
                 + "</body></html>";
        }
    }
}