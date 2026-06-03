using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Local_Service_marketPlace.Models.ViewModels
{
    public class ProviderEditProfileViewModel
    {
        [MaxLength(50)]
        [Required(ErrorMessage = "First name is required.")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [MaxLength(50)]
        [Required(ErrorMessage = "Last name is required.")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [MaxLength(500)]
        [Display(Name = "Bio")]
        public string? Bio { get; set; }

        [Required]
        [Range(0, 60)]
        [Display(Name = "Years of Experience")]
        public int YearsOfExperience { get; set; }

        [Required]
        [Display(Name = "Available From")]
        public TimeSpan AvailableFrom { get; set; }

        [Required]
        [Display(Name = "Available To")]
        public TimeSpan AvailableTo { get; set; }

        [Display(Name = "Profile Picture")]
        public IFormFile? ProfileImageFile { get; set; }

        public string? ExistingProfileImage { get; set; }

        [Display(Name = "Service Categories")]
        public List<int> SelectedCategoryIds { get; set; } = new();

        public List<SelectListItem> Categories { get; set; } = new();
    }
}