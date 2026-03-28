using System.ComponentModel.DataAnnotations;

namespace JayamaliOptical.Web.Models
{
    public class Review
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Location { get; set; }

        [Required, Range(1, 5)]
        public int Rating { get; set; } = 5;

        [Required, StringLength(1000)]
        public string Comment { get; set; } = string.Empty;

        // Admin can optionally set a service type label
        [StringLength(60)]
        public string? ServiceType { get; set; }

        public bool IsApproved { get; set; } = false;
        public bool IsFeatured { get; set; } = false;
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    }
}