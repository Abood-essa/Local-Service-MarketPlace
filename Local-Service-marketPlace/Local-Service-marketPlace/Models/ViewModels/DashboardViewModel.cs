namespace Local_Service_marketPlace.Models.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalUsers { get; set; }

        public int TotalProviders { get; set; }

        public int TotalRequests { get; set; }

        public int TotalBookings { get; set; }

        public int PendingComplaints { get; set; }

        public decimal TotalRevenue { get; set; }

        public List<ServiceRequest> LatestRequests { get; set; }
            = new();

        public List<Booking> LatestBookings { get; set; }
            = new();
    }
}