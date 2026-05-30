using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Local_Service_marketPlace.Models
{
    public class Review
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        public string CustomerId { get; set; }
        public int ProviderProfileId { get; set; }
        [Range(1, 5)] public int Rating { get; set; }
        [MaxLength(500)] public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        [ForeignKey("BookingId")]
        public Booking Booking { get; set; }
        [ForeignKey("CustomerId")]
        public ApplicationUser Customer { get; set; }
        [ForeignKey("ProviderProfileId")]
        public ProviderProfile ProviderProfile { get; set; }
    }
}
