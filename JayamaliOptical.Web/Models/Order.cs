using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JayamaliOptical.Web.Models
{
    public class Order
    {
        public int Id { get; set; }

        public string OrderNumber { get; set; } = string.Empty;

        public string? UserId { get; set; }
        public IdentityUser? User { get; set; }

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

        [Required(ErrorMessage = "Address is required")]
        [StringLength(500)]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "City is required")]
        [StringLength(100)]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "Postal code is required")]
        [StringLength(20)]
        public string PostalCode { get; set; } = string.Empty;

        public decimal TotalAmount { get; set; }

        public string? OrderNotes { get; set; }

        public string Status { get; set; } = "Pending";

        /// <summary>COD or PayHere</summary>
        [StringLength(50)]
        public string PaymentMethod { get; set; } = "COD";

        /// <summary>Pending | Paid | Failed | Refunded</summary>
        [StringLength(50)]
        public string PaymentStatus { get; set; } = "Pending";

        /// <summary>PayHere transaction/order reference returned by IPN</summary>
        [StringLength(100)]
        public string? PaymentReference { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public DateTime? ShippedDate { get; set; }
        public DateTime? DeliveredDate { get; set; }

        // Prescription — option 1: saved prescription FK
        [ForeignKey("Prescription")]
        public int? PrescriptionId { get; set; }
        public Prescription? Prescription { get; set; }

        // Prescription — option 2: uploaded image
        [StringLength(500)]
        public string? PrescriptionImagePath { get; set; }
        [StringLength(255)]
        public string? PrescriptionFileName { get; set; }

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

        [Required(ErrorMessage = "Address is required")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "City is required")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "Postal code is required")]
        public string PostalCode { get; set; } = string.Empty;

        public string? OrderNotes { get; set; }

        public ShoppingCart Cart { get; set; } = new ShoppingCart();
        public decimal TotalAmount => Cart.TotalPrice;

        public bool CartRequiresPrescription { get; set; } = false;
        public int? SelectedPrescriptionId { get; set; } = null;
        public IFormFile? UploadPrescriptionFile { get; set; }
        public List<Prescription>? UserPrescriptions { get; set; }

        // Payment
        public string PaymentMethod { get; set; } = "COD";
        public bool CodEnabled { get; set; } = true;
        public bool PayHereEnabled { get; set; } = false;
    }
}