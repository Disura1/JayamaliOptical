using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JayamaliOptical.Web.Models
{
    public class Order
    {
        public int Id { get; set; }

        // Order Number (Auto-generated)
        public string OrderNumber { get; set; } = string.Empty;

        // User Information (for logged-in users)
        public string? UserId { get; set; }  // Nullable for guest orders
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

        // Billing Address
        [Required(ErrorMessage = "Address is required")]
        [StringLength(500)]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "City is required")]
        [StringLength(100)]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "Postal code is required")]
        [StringLength(20)]
        public string PostalCode { get; set; } = string.Empty;

        // Order Details
        public decimal TotalAmount { get; set; }

        public string? OrderNotes { get; set; }

        // Order Status
        public string Status { get; set; } = "Pending"; // Pending, Confirmed, Processing, Shipped, Delivered, Cancelled

        // Timestamps
        public DateTime OrderDate { get; set; } = DateTime.Now;
        public DateTime? ShippedDate { get; set; }
        public DateTime? DeliveredDate { get; set; }

        // Navigation Properties
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }

    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public Order? Order { get; set; }

        public int ProductId { get; set; }
        public Product? Product { get; set; }

        public string ProductName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal Total => Price * Quantity;
    }

    public class CheckoutViewModel
    {
        // Customer Info
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

        // Address
        [Required(ErrorMessage = "Address is required")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "City is required")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "Postal code is required")]
        public string PostalCode { get; set; } = string.Empty;

        // Additional
        public string? OrderNotes { get; set; }

        // Read-only properties
        public ShoppingCart Cart { get; set; } = new ShoppingCart();
        public decimal TotalAmount => Cart.TotalPrice;

        // ADD THESE: Prescription handling
        public bool CartRequiresPrescription { get; set; } = false;
        public int? SelectedPrescriptionId { get; set; } = null;
        public IFormFile? UploadPrescriptionFile { get; set; }

        // List of user's prescriptions (for dropdown)
        public List<Prescription>? UserPrescriptions { get; set; }
    }
}