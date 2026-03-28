using JayamaliOptical.Web.Services;
using System.ComponentModel.DataAnnotations;

namespace JayamaliOptical.Web.Models
{
    public class GalleryImage
    {
        public int Id { get; set; }

        [Required, StringLength(255)]
        public string ImagePath { get; set; } = string.Empty;

        [StringLength(150)]
        public string? Caption { get; set; }

        [StringLength(100)]
        public string? Category { get; set; }

        public int DisplayOrder { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = TimeHelper.Now;
    }
}