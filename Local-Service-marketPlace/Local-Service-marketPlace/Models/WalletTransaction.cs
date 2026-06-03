using System.ComponentModel.DataAnnotations.Schema;

namespace Local_Service_marketPlace.Models
{
    public enum TransactionType
    {
        TopUp,
        CommissionDeduction
    }

    public class WalletTransaction
    {
        public int Id { get; set; }

        public int ProviderProfileId { get; set; }

        public TransactionType Type { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal BalanceAfter { get; set; }

        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation
        [ForeignKey("ProviderProfileId")]
        public ProviderProfile ProviderProfile { get; set; }

        // Optional — link deductions to a booking
        public int? BookingId { get; set; }
        public Booking? Booking { get; set; }
    }
}