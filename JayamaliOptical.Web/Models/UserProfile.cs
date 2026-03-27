using JayamaliOptical.Web.Services;
using System.ComponentModel.DataAnnotations;

namespace JayamaliOptical.Web.Models
{
    public class UserProfile
    {
        public int Id { get; set; }

        // Link to user (nullable for guest profiles)
        public string? UserId { get; set; }

        // Email is the primary identifier (works for guests too)
        [Required]
        [EmailAddress]
        [StringLength(256)]  // ← ADD THIS: Limits to 256 characters
        public string Email { get; set; } = string.Empty;

        // Customer Information
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [StringLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        // Address Information
        [StringLength(500)]
        public string Address { get; set; } = string.Empty;

        [StringLength(100)]
        public string City { get; set; } = string.Empty;

        [StringLength(20)]
        public string PostalCode { get; set; } = string.Empty;

        // Metadata
        public DateTime CreatedDate { get; set; } = TimeHelper.Now;
        public DateTime? LastUsedDate { get; set; }
        public int UsageCount { get; set; } = 0;

        // Is this the default profile for this user?
        public bool IsDefault { get; set; } = false;
    }
}