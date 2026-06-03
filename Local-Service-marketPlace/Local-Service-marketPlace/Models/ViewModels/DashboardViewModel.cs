namespace Local_Service_marketPlace.Models.ViewModels
{
    public class DashboardViewModel
    {
        // Users
        public int TotalUsers { get; set; }
        public int TotalProviders { get; set; }
        public int VerifiedProviders { get; set; }
        public int PendingVerification { get; set; }

        // Requests & Bookings
        public int TotalRequests { get; set; }
        public int TotalBookings { get; set; }
        public int ActiveBookings { get; set; }
        public int CompletedBookings { get; set; }

        // Complaints
        public int PendingComplaints { get; set; }

        // Financial
        public decimal TotalRevenue { get; set; }
        public decimal TotalWalletCommissions { get; set; }
        public decimal TotalNegativeBalances { get; set; }
        public int ProvidersInDebt { get; set; }

        // Latest
        public List<ServiceRequest> LatestRequests { get; set; } = new();
        public List<Booking> LatestBookings { get; set; } = new();
    }
}