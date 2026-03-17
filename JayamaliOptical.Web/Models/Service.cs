using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JayamaliOptical.Web.Models
{
    public class Service
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? Price { get; set; }

        public int? DurationMinutes { get; set; }

        public bool RequiresAppointment { get; set; } = false;

        // REMOVE this:
        // public string? Icon { get; set; }

        // ADD these for image support:
        public string? ImageUrl { get; set; }  // Path to service image

        // Custom buttons
        public string? Button1Text { get; set; }
        public string? Button1Url { get; set; }
        public string? Button1Class { get; set; }

        public string? Button2Text { get; set; }
        public string? Button2Url { get; set; }
        public string? Button2Class { get; set; }

        public string? Button3Text { get; set; }
        public string? Button3Url { get; set; }
        public string? Button3Class { get; set; }

        public int DisplayOrder { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}