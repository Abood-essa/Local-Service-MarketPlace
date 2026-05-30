using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Local_Service_marketPlace.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        [Required] public string Title { get; set; }
        [Required] public string Message { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }
    }
}
