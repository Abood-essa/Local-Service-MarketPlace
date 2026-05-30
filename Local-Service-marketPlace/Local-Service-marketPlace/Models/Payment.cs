using Local_Service_marketPlace.Models.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Local_Service_marketPlace.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        [Column(TypeName = "decimal(18,2)")] public decimal Amount { get; set; }
        [Column(TypeName = "decimal(18,2)")] public decimal CommissionAmount { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public DateTime TransactionDate { get; set; } = DateTime.Now;
        [ForeignKey("BookingId")]
        public Booking Booking { get; set; }
    }
}
