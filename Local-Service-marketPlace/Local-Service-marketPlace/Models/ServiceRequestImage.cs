using System.ComponentModel.DataAnnotations.Schema;

namespace Local_Service_marketPlace.Models
{
    public class ServiceRequestImage
    {
        public int Id { get; set; }
        public int ServiceRequestId { get; set; }
        public string ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        [ForeignKey("ServiceRequestId")]
        public ServiceRequest ServiceRequest { get; set; }
    }
}
