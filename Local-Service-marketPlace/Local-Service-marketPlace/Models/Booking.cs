using Local_Service_marketPlace.Models.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Local_Service_marketPlace.Models
{
    public class Booking
    {
        public int Id { get; set; }
        public int ServiceRequestId { get; set; }
        public int ServiceOfferId { get; set; }
        public string CustomerId { get; set; }
        public int ProviderProfileId { get; set; }
        [Column(TypeName = "decimal(18,2)")] public decimal FinalConfirmedPrice { get; set; }
        public double CommissionPercentage { get; set; }
        [Column(TypeName = "decimal(18,2)")] public decimal CommissionAmount { get; set; }
        [Column(TypeName = "decimal(18,2)")] public decimal ProviderEarnings { get; set; }
        public BookingStatus BookingStatus { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        [ForeignKey("ServiceRequestId")]
        public ServiceRequest ServiceRequest { get; set; }
        [ForeignKey("ServiceOfferId")]
        public ServiceOffer ServiceOffer { get; set; }
        [ForeignKey("CustomerId")]
        public ApplicationUser Customer { get; set; }
        [ForeignKey("ProviderProfileId")]
        public ProviderProfile ProviderProfile { get; set; }
        public Review Review { get; set; }
        public Payment Payment { get; set; }
    }
}
