using System.ComponentModel.DataAnnotations;

namespace Local_Service_marketPlace.Models.ViewModels
{
    public class ContactViewModel
    {
        [Required(ErrorMessage = "Please enter your name")]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required(ErrorMessage = "Please enter your email address")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        [MaxLength(100)]
        public string Email { get; set; }

        [Required(ErrorMessage = "Please enter a subject")]
        [MaxLength(150)]
        public string Subject { get; set; }

        [Required(ErrorMessage = "Please enter your message")]
        [MaxLength(2000)]
        public string Message { get; set; }
    }
}
