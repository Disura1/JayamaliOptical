using System.ComponentModel.DataAnnotations;

namespace JayamaliOptical.Web.Models
{
    public class SiteSettings
    {
        public int Id { get; set; }

        // Contact Info
        [StringLength(200)] public string Address { get; set; } = string.Empty;
        [StringLength(20)] public string Phone1 { get; set; } = string.Empty;
        [StringLength(20)] public string Phone2 { get; set; } = string.Empty;
        [StringLength(20)] public string Phone3 { get; set; } = string.Empty;
        [StringLength(100)] public string Email { get; set; } = string.Empty;

        // Social Media
        [StringLength(300)] public string FacebookUrl { get; set; } = string.Empty;
        [StringLength(300)] public string WhatsAppUrl { get; set; } = string.Empty;
        [StringLength(300)] public string TikTokUrl { get; set; } = string.Empty;
        [StringLength(300)] public string InstagramUrl { get; set; } = string.Empty;

        // Privacy Policy (max 5000 chars to protect structure)
        [StringLength(5000)] public string PrivacyPolicy { get; set; } = string.Empty;

        // History (max 3000 chars)
        [StringLength(3000)] public string History { get; set; } = string.Empty;

        // Vision (max 1000 chars)
        [StringLength(1000)] public string Vision { get; set; } = string.Empty;

        // Mission (max 2000 chars)
        [StringLength(2000)] public string Mission { get; set; } = string.Empty;
    }
}