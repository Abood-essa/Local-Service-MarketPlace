using System.ComponentModel.DataAnnotations;

namespace Local_Service_marketPlace.Models
{
    public class Category
    {

        public int Id { get; set; }
        [Required][MaxLength(100)] public string Name { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public ICollection<ServiceRequest> ServiceRequests { get; set; }
        public ICollection<ProviderCategory> ProviderCategories { get; set; }
    }
}
