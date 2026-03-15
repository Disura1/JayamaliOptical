using System.ComponentModel.DataAnnotations;

namespace JayamaliOptical.Web.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation property
        public ICollection<Product>? Products { get; set; }
    }
}