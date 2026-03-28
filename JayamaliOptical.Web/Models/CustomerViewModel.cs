namespace JayamaliOptical.Web.Models
{
    public class CustomerViewModel
    {
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public int TotalOrders { get; set; }
        public int TotalBookings { get; set; }
        public decimal TotalSpent { get; set; }
        public DateTime FirstOrderDate { get; set; }
        public DateTime LastOrderDate { get; set; }
        public bool IsRegistered { get; set; }
    }

    public class CustomerDetailViewModel
    {
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public bool IsRegistered { get; set; }
        public string? RegisteredUserId { get; set; }
        public List<Order> Orders { get; set; } = new();
        public List<ServiceBooking> Bookings { get; set; } = new();
        public List<Prescription> Prescriptions { get; set; } = new();
        public decimal TotalSpent { get; set; }
        public DateTime FirstOrderDate { get; set; }
        public DateTime LastOrderDate { get; set; }
    }
}