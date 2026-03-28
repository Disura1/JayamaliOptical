namespace JayamaliOptical.Web.Models
{
    public class DashboardViewModel
    {
        public int TotalCustomers { get; set; }
        public int TotalOrders { get; set; }
        public int TotalProducts { get; set; }
        public int TodayAppointments { get; set; }
        public decimal TotalRevenue { get; set; }

        // Orders
        public int PendingOrders { get; set; }
        public int DeliveredOrders { get; set; }
        public decimal MonthRevenue { get; set; }

        // Appointments
        public int PendingAppointments { get; set; }
        public int TotalAppointments { get; set; }
        public int UpcomingAppointments { get; set; }

        // Content
        public int TotalReviews { get; set; }
        public int PendingReviews { get; set; }
        public int UnreadMessages { get; set; }
        public int ActiveProducts { get; set; }

        // Recent data
        public List<JayamaliOptical.Web.Models.Order> RecentOrders { get; set; } = new();
        public List<JayamaliOptical.Web.Models.ServiceBooking> UpcomingBookings { get; set; } = new();
    }
}