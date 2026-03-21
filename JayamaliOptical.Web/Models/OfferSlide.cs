using System.ComponentModel.DataAnnotations;

namespace JayamaliOptical.Web.Models
{
    public class OfferSlide
    {
        public int Id { get; set; }

        [StringLength(255)]
        public string ImagePath { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Caption { get; set; }

        [StringLength(200)]
        public string? LinkUrl { get; set; }

        public int DisplayOrder { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}