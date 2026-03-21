using Microsoft.EntityFrameworkCore.Migrations;
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

        // Opening Hours — each day has open/close time + closed flag
        [StringLength(10)] public string MonOpen { get; set; } = "08:30";
        [StringLength(10)] public string MonClose { get; set; } = "18:00";
        public bool MonClosed { get; set; } = false;

        [StringLength(10)] public string TueOpen { get; set; } = "08:30";
        [StringLength(10)] public string TueClose { get; set; } = "18:00";
        public bool TueClosed { get; set; } = false;

        [StringLength(10)] public string WedOpen { get; set; } = "08:30";
        [StringLength(10)] public string WedClose { get; set; } = "18:00";
        public bool WedClosed { get; set; } = false;

        [StringLength(10)] public string ThuOpen { get; set; } = "08:30";
        [StringLength(10)] public string ThuClose { get; set; } = "18:00";
        public bool ThuClosed { get; set; } = false;

        [StringLength(10)] public string FriOpen { get; set; } = "08:30";
        [StringLength(10)] public string FriClose { get; set; } = "18:00";
        public bool FriClosed { get; set; } = false;

        [StringLength(10)] public string SatOpen { get; set; } = "08:30";
        [StringLength(10)] public string SatClose { get; set; } = "16:00";
        public bool SatClosed { get; set; } = false;

        [StringLength(10)] public string SunOpen { get; set; } = "08:30";
        [StringLength(10)] public string SunClose { get; set; } = "18:00";
        public bool SunClosed { get; set; } = true;

        [StringLength(10)] public string HolOpen { get; set; } = "08:30";
        [StringLength(10)] public string HolClose { get; set; } = "18:00";
        public bool HolClosed { get; set; } = true;

        // Helper: is clinic open right now?
        public bool IsOpenNow()
        {
            var now = DateTime.Now;
            var open = string.Empty;
            var close = string.Empty;
            bool closed = false;

            switch (now.DayOfWeek)
            {
                case DayOfWeek.Monday: open = MonOpen; close = MonClose; closed = MonClosed; break;
                case DayOfWeek.Tuesday: open = TueOpen; close = TueClose; closed = TueClosed; break;
                case DayOfWeek.Wednesday: open = WedOpen; close = WedClose; closed = WedClosed; break;
                case DayOfWeek.Thursday: open = ThuOpen; close = ThuClose; closed = ThuClosed; break;
                case DayOfWeek.Friday: open = FriOpen; close = FriClose; closed = FriClosed; break;
                case DayOfWeek.Saturday: open = SatOpen; close = SatClose; closed = SatClosed; break;
                case DayOfWeek.Sunday: open = SunOpen; close = SunClose; closed = SunClosed; break;
            }

            if (closed) return false;
            if (TimeSpan.TryParse(open, out var openTime) && TimeSpan.TryParse(close, out var closeTime))
                return now.TimeOfDay >= openTime && now.TimeOfDay < closeTime;

            return false;
        }

        // Privacy Policy (max 5000 chars)
        [StringLength(5000)] public string PrivacyPolicy { get; set; } = string.Empty;

        // History (max 3000 chars)
        [StringLength(3000)] public string History { get; set; } = string.Empty;

        // Vision (max 1000 chars)
        [StringLength(1000)] public string Vision { get; set; } = string.Empty;

        // Mission (max 2000 chars)
        [StringLength(2000)] public string Mission { get; set; } = string.Empty;
    }
}