using Local_Service_marketPlace.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Local_Service_marketPlace.Models
{
    public class ServiceRequest
    {
        public int Id { get; set; }
        [Required] public string CustomerId { get; set; }
        public int CategoryId { get; set; }
        [Required][MaxLength(150)] public string Title { get; set; }
        [Required] public string Description { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public DateTime PreferredDate { get; set; }
        public ServiceRequestStatus Status { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now; public DateTime? UpdatedAt { get; set; }
        [ForeignKey("CustomerId")]
        public ApplicationUser Customer { get; set; }
        [ForeignKey("CategoryId")]
        public Category Category { get; set; }
        public ICollection<ServiceOffer> Offers { get; set; }
        public ICollection<ServiceRequestImage> Images { get; set; }

        public Booking Booking { get; set; }
    }
}
