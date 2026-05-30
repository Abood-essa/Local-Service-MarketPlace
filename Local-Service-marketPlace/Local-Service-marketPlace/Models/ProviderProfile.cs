using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Local_Service_marketPlace.Models
{
    public class ProviderProfile
    {

        public int Id { get; set; }
        [Required] public string UserId { get; set; }
        public string? Bio { get; set; }
        public int YearsOfExperience { get; set; }
        public string? NationalIdImage { get; set; }
        public double AverageRating { get; set; }
        public int CompletedJobsCount { get; set; }
        public bool IsVerified { get; set; }
        public TimeSpan AvailableFrom { get; set; }
        public TimeSpan AvailableTo { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }
        public ICollection<ServiceOffer> Offers { get; set; }
        public ICollection<ProviderCategory> ProviderCategories { get; set; }
        public ICollection<Review> ReviewsReceived { get; set; }
        public ICollection<Booking> Bookings { get; set; }
    }
}
