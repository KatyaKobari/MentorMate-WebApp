using System.ComponentModel.DataAnnotations;

namespace MentorMate.ViewModels
{
    public class MenteeProfileViewModel
    {
        public int MenteeId { get; set; }

        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "Gender")]
        public string Gender { get; set; }

        [Display(Name = "LinkedIn URL")]
        public string LinkedInUrl { get; set; }

        [Display(Name = "Field of Study")]
        public string FieldOfStudy { get; set; }

        [Display(Name = "Bio")]
        public string Bio { get; set; }

        [Display(Name = "Interests")]
        public string Interests { get; set; }


        public string AvatarUrl { get; set; }
        public int SessionCount { get; set; }

        // للتحويل من string إلى array والعكس
        public string[] InterestsArray =>
            !string.IsNullOrEmpty(Interests) ? Interests.Split(',') : new string[0];
    }

    public class EditMenteeProfileViewModel
    {
        [Required(ErrorMessage = "Full name is required")]
        [MaxLength(100, ErrorMessage = "Full name cannot exceed 100 characters")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Gender is required")]
        [Display(Name = "Gender")]
        public string Gender { get; set; }

        [Url(ErrorMessage = "Invalid LinkedIn URL")]
        [Display(Name = "LinkedIn URL")]
        public string LinkedInUrl { get; set; }

        [Required(ErrorMessage = "Field of study is required")]
        [MaxLength(200, ErrorMessage = "Field of study cannot exceed 200 characters")]
        [Display(Name = "Field of Study")]
        public string FieldOfStudy { get; set; }

        [MaxLength(500, ErrorMessage = "Bio cannot exceed 500 characters")]
        [Display(Name = "About Me")]
        public string Bio { get; set; }

        [MaxLength(300, ErrorMessage = "Interests cannot exceed 300 characters")]
        [Display(Name = "Interests (comma separated)")]
        public string Interests { get; set; }

    }
}