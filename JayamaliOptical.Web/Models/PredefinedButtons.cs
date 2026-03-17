namespace JayamaliOptical.Web.Models
{
    public static class PredefinedButtons
    {
        public static readonly List<ServiceButtonTemplate> Buttons = new()
        {
            new ServiceButtonTemplate
            {
                Id = "book_appointment",
                Text = "Book Appointment",
                Url = "/Appointments/Create",
                Class = "btn-success",
                Icon = "fas fa-calendar-plus"
            },
            new ServiceButtonTemplate
            {
                Id = "contact_clinic",
                Text = "Contact Clinic",
                Url = "/Contact",
                Class = "btn-info",
                Icon = "fas fa-phone"
            },
            new ServiceButtonTemplate
            {
                Id = "learn_more",
                Text = "Learn More",
                Url = "/Services/Details/{id}",
                Class = "btn-primary",
                Icon = "fas fa-info-circle"
            },
            new ServiceButtonTemplate
            {
                Id = "view_pricing",
                Text = "View Pricing",
                Url = "/Services/Pricing",
                Class = "btn-warning",
                Icon = "fas fa-tag"
            },
            new ServiceButtonTemplate
            {
                Id = "call_now",
                Text = "Call Now",
                Url = "tel:+94112345678",
                Class = "btn-danger",
                Icon = "fas fa-phone-volume"
            },
            new ServiceButtonTemplate
            {
                Id = "whatsapp_us",
                Text = "WhatsApp Us",
                Url = "https://wa.me/94771234567",
                Class = "btn-success",
                Icon = "fab fa-whatsapp"
            },
            new ServiceButtonTemplate
            {
                Id = "free_consultation",
                Text = "Free Consultation",
                Url = "/Appointments/Create",
                Class = "btn-success",
                Icon = "fas fa-user-md"
            },
            new ServiceButtonTemplate
            {
                Id = "eligibility_check",
                Text = "Eligibility Check",
                Url = "/Services/Eligibility",
                Class = "btn-info",
                Icon = "fas fa-clipboard-check"
            },
            new ServiceButtonTemplate
            {
                Id = "view_doctors",
                Text = "View Doctors",
                Url = "/Doctors",
                Class = "btn-info",
                Icon = "fas fa-user-md"
            },
            new ServiceButtonTemplate
            {
                Id = "insurance_info",
                Text = "Insurance Info",
                Url = "/Insurance",
                Class = "btn-warning",
                Icon = "fas fa-shield-alt"
            },
            new ServiceButtonTemplate
            {
                Id = "walk_in",
                Text = "Walk-in Available",
                Url = "/Services",
                Class = "btn-info",
                Icon = "fas fa-walking"
            },
            new ServiceButtonTemplate
            {
                Id = "consult_surgeon",
                Text = "Consult Surgeon",
                Url = "/Appointments/Create",
                Class = "btn-danger",
                Icon = "fas fa-procedures"
            }
        };
    }

    public class ServiceButtonTemplate
    {
        public string Id { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string Class { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
    }
}