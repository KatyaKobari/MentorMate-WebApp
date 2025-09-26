using System.ComponentModel.DataAnnotations;

namespace MentorMate.ViewModels
{
    public class RegisterStep3ViewModel
    {
        [Required(ErrorMessage = "Please select a role")]
        public string Role { get; set; }

        // Common fields for both roles
        [Required(ErrorMessage = "Bio is required")]
        [MaxLength(500, ErrorMessage = "Bio cannot exceed 500 characters")]
        [Display(Name = "About You")]
        public string Bio { get; set; }

        [MaxLength(200, ErrorMessage = "LinkedIn URL cannot exceed 200 characters")]
        [Url(ErrorMessage = "Please enter a valid URL")]
        [Display(Name = "LinkedIn Profile")]
        public string LinkedInUrl { get; set; }

        // Mentor specific fields - جعلها غير مطلوبة
        [MaxLength(100, ErrorMessage = "Expertise cannot exceed 100 characters")]
        [Display(Name = "Area of Expertise")]
        public string Expertise { get; set; }

        [MaxLength(200, ErrorMessage = "Skills cannot exceed 200 characters")]
        [Display(Name = "Skills")]
        public string Skills { get; set; }

        [Range(0, 50, ErrorMessage = "Years of experience must be between 0 and 50")]
        [Display(Name = "Years of Experience")]
        public int? YearsOfExperience { get; set; }

        [MaxLength(50, ErrorMessage = "Availability cannot exceed 50 characters")]
        [Display(Name = "Availability")]
        public string Availability { get; set; }

        // Mentee specific fields
        [Required(ErrorMessage = "Field of study is required for mentees")]
        [MaxLength(200, ErrorMessage = "Field of Study cannot exceed 200 characters")]
        [Display(Name = "Field of Study")]
        public string FieldOfStudy { get; set; }

        [Required(ErrorMessage = "Interests are required for mentees")]
        [MaxLength(300, ErrorMessage = "Interests cannot exceed 300 characters")]
        [Display(Name = "Interests")]
        public string Interests { get; set; }

        [Required(ErrorMessage = "Goals are required for mentees")]
        [MaxLength(500, ErrorMessage = "Goals cannot exceed 500 characters")]
        [Display(Name = "Goals")]
        public string Goals { get; set; }
    }
}