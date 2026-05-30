using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Local_Service_marketPlace.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required][MaxLength(50)] public string FirstName { get; set; }
        [Required][MaxLength(50)] public string LastName { get; set; }
        public string? ProfileImage { get; set; }
        [MaxLength(255)] public string? Address { get; set; }
        [MaxLength(100)] public string? City { get; set; }
        public bool IsActive { get; set; } = true; public DateTime CreatedAt { get; set; } = DateTime.Now;
        public ProviderProfile ProviderProfile { get; set; }
        public ICollection<ServiceRequest> CustomerRequests { get; set; }
        public ICollection<Booking> CustomerBookings { get; set; }
        public ICollection<Notification> Notifications { get; set; }
        public ICollection<Complaint> Complaints { get; set; }
    }
}
