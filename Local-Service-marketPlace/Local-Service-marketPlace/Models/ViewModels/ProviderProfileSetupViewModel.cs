using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Local_Service_marketPlace.Models.ViewModels
{
    public class ProviderProfileSetupViewModel
    {
        [MaxLength(500)]
        [Display(Name = "Bio")]
        public string? Bio { get; set; }

        [Required(ErrorMessage = "Years of experience is required.")]
        [Range(0, 60, ErrorMessage = "Please enter a valid number.")]
        [Display(Name = "Years of Experience")]
        public int YearsOfExperience { get; set; }

        [Required(ErrorMessage = "Please set your available from time.")]
        [Display(Name = "Available From")]
        public TimeSpan AvailableFrom { get; set; }

        [Required(ErrorMessage = "Please set your available to time.")]
        [Display(Name = "Available To")]
        public TimeSpan AvailableTo { get; set; }

        [Required(ErrorMessage = "Please upload your National ID image.")]
        [Display(Name = "National ID Image")]
        public IFormFile NationalIdImage { get; set; }

        [Required(ErrorMessage = "Please select at least one category.")]
        [Display(Name = "Service Categories")]
        public List<int> SelectedCategoryIds { get; set; } = new();

        // Populated in controller
        public List<SelectListItem> Categories { get; set; } = new();
    }
}