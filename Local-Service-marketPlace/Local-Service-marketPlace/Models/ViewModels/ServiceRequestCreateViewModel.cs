using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Local_Service_marketPlace.Models.ViewModels
{
    public class ServiceRequestCreateViewModel
    {
        [Required(ErrorMessage = "Please select a category.")]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Please enter a title.")]
        [MaxLength(150)]
        [Display(Name = "Title")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Please describe the job.")]
        [Display(Name = "Description")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Please enter the address.")]
        [Display(Name = "Address")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Please enter the city.")]
        [Display(Name = "City")]
        public string City { get; set; }

        [Required(ErrorMessage = "Please choose a preferred date.")]
        [DataType(DataType.Date)]
        [Display(Name = "Preferred Date")]
        public DateTime PreferredDate { get; set; } = DateTime.Today.AddDays(1);

        [Display(Name = "Images (optional)")]
        public List<IFormFile>? Images { get; set; }

        // Populated in controller for the dropdown
        public List<SelectListItem> Categories { get; set; } = new();
    }
}
