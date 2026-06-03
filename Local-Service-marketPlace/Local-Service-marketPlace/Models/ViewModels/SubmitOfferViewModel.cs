using System.ComponentModel.DataAnnotations;

namespace Local_Service_marketPlace.Models.ViewModels
{
    public class SubmitOfferViewModel
    {
        public int ServiceRequestId { get; set; }

        [Required(ErrorMessage = "Minimum price is required.")]
        [Range(1, 99999, ErrorMessage = "Enter a valid price.")]
        [Display(Name = "Minimum Estimated Price (JD)")]
        public decimal EstimatedMinPrice { get; set; }

        [Required(ErrorMessage = "Maximum price is required.")]
        [Range(1, 99999, ErrorMessage = "Enter a valid price.")]
        [Display(Name = "Maximum Estimated Price (JD)")]
        public decimal EstimatedMaxPrice { get; set; }

        [MaxLength(500)]
        [Display(Name = "Notes (optional)")]
        public string? Notes { get; set; }
    }
}