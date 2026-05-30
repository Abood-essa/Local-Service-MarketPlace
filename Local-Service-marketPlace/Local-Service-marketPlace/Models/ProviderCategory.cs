using System.ComponentModel.DataAnnotations.Schema;

namespace Local_Service_marketPlace.Models
{
    public class ProviderCategory
    {

        public int Id { get; set; }
        public int ProviderProfileId { get; set; }
        public int CategoryId { get; set; }
        [ForeignKey("ProviderProfileId")]
        public ProviderProfile ProviderProfile { get; set; }
        [ForeignKey("CategoryId")]
        public Category Category { get; set; }
    }
}
