using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JayamaliOptical.Web.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        // Foreign key to Category
        public int CategoryId { get; set; }

        // Navigation property
        public Category? Category { get; set; }

        // Optical-specific properties
        public string? Brand { get; set; }

        // For spectacles: Frame type (Full Rim, Half Rim, Rimless)
        public string? FrameType { get; set; }

        // For lenses: Power information
        public string? LensPower { get; set; }

        // For IOL Lens: Type (Monofocal, Multifocal, Toric)
        public string? IOLType { get; set; }

        public int StockQuantity { get; set; }

        public string? ImageUrl { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}