using Local_Service_marketPlace.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Local_Service_marketPlace.Models
{
    public class Complaint
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        public string ReportedByUserId { get; set; }
        [Required] public string Reason { get; set; }
        [Required] public string Description { get; set; }
        public string? AdminNotes { get; set; }
        public ComplaintStatus Status { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now; public DateTime? ResolvedAt { get; set; }
        [ForeignKey("BookingId")]
        public Booking Booking { get; set; }
        [ForeignKey("ReportedByUserId")]
        public ApplicationUser ReportedByUser { get; set; }
    }
}
