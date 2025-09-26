using System.ComponentModel.DataAnnotations;

namespace MentorMate.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Full name is required")]
        [MaxLength(100)]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Enter a valid email")]
        [MaxLength(150)]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Confirm your password")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Select a role")]
        public string Role { get; set; }  // "Mentor" or "Mentee"

        // Bio and LinkedIn (for all users)
        [MaxLength(500)]
        public string Bio { get; set; }

        [MaxLength(200)]
        [Url(ErrorMessage = "Enter a valid URL")]
        public string LinkedInUrl { get; set; }

        // Mentor fields
        public string Expertise { get; set; }
        public string Skills { get; set; }
        public int? YearsOfExperience { get; set; }

        // Mentee fields
        public string Interests { get; set; }
        public string Goals { get; set; }
    }
}
