using Local_Service_marketPlace.Models.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Local_Service_marketPlace.Models
{
    public class ServiceOffer
    {

        public int Id { get; set; }
        public int ServiceRequestId { get; set; }
        public int ProviderProfileId { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal EstimatedMinPrice { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal EstimatedMaxPrice { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal? FinalPrice { get; set; }
        public string? Notes { get; set; }
        public OfferStatus Status { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now; public DateTime? UpdatedAt { get; set; }
        [ForeignKey("ServiceRequestId")]
        public ServiceRequest ServiceRequest { get; set; }
        [ForeignKey("ProviderProfileId")]
        public ProviderProfile ProviderProfile { get; set; }

        public Booking Booking { get; set; }
    }
}
