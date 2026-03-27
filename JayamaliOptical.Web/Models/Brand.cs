using JayamaliOptical.Web.Services;
using System.ComponentModel.DataAnnotations;

namespace JayamaliOptical.Web.Models
{
    public class Brand
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(255)]
        public string? LogoPath { get; set; }

        public int DisplayOrder { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = TimeHelper.Now;
    }
}