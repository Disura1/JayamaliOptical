using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JayamaliOptical.Web.Models
{
    public class ServiceBooking
    {
        public int Id { get; set; }

        // Booking Reference Number
        public string BookingNumber { get; set; } = string.Empty;

        // Service Information
        public int ServiceId { get; set; }
        public Service? Service { get; set; }

        // User Information (for logged-in users)
        public string? UserId { get; set; }  // Nullable for guest bookings
        public IdentityUser? User { get; set; }

        // Customer Information
        [Required(ErrorMessage = "First name is required")]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number")]
        [StringLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        // Appointment Details
        [Required(ErrorMessage = "Appointment date is required")]
        [DataType(DataType.Date)]
        public DateTime AppointmentDate { get; set; }

        [Required(ErrorMessage = "Appointment time is required")]
        [DataType(DataType.Time)]
        public TimeSpan AppointmentTime { get; set; }

        // Additional Information
        [StringLength(500)]
        public string? Notes { get; set; }

        // Booking Status
        public string Status { get; set; } = "Pending"; // Pending, Confirmed, Completed, Cancelled, NoShow

        // Timestamps
        public DateTime BookingDate { get; set; } = DateTime.Now;

        // Admin Notes
        [StringLength(500)]
        public string? AdminNotes { get; set; }
    }

    public class ServiceBookingViewModel
    {
        public int ServiceId { get; set; }
        public string? ServiceName { get; set; }

        [Required(ErrorMessage = "First name is required")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required")]
        [Phone]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Appointment date is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Preferred Date")]
        public DateTime AppointmentDate { get; set; } = DateTime.Now.AddDays(1);

        [Required(ErrorMessage = "Appointment time is required")]
        [Display(Name = "Preferred Time")]
        public string AppointmentTime { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "Additional Notes")]
        public string? Notes { get; set; }
    }
}