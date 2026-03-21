using System.ComponentModel.DataAnnotations;

namespace JayamaliOptical.Web.Models
{
    public class ContactMessage
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(20)]
        public string? Phone { get; set; }

        [Required, StringLength(100), EmailAddress]
        public string Email { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Subject { get; set; }

        [Required, StringLength(2000)]
        public string Message { get; set; } = string.Empty;

        public DateTime SentAt { get; set; } = DateTime.Now;

        public bool IsRead { get; set; } = false;
    }
}