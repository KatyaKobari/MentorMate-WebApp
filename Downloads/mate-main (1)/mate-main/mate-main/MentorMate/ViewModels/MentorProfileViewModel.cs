using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MentorMate.ViewModels
{
    public class MentorProfileViewModel
    {
        public int MentorId { get; set; }

        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "Gender")]
        public string Gender { get; set; }

        [Display(Name = "LinkedIn URL")]
        public string LinkedInUrl { get; set; }

        [Display(Name = "Expertise")]
        public string Expertise { get; set; }

        [Display(Name = "Skills")]
        public string Skills { get; set; }

        [Display(Name = "Years of Experience")]
        public int YearsOfExperience { get; set; }

        [Display(Name = "Bio")]
        public string Bio { get; set; }

        [Display(Name = "Rating")]
        public decimal Rating { get; set; }

        [Display(Name = "Review Count")]
        public int ReviewCount { get; set; }

        [Display(Name = "Availability")]
        public string Availability { get; set; } = "Available";

        public string AvatarUrl { get; set; }
        public int MenteeCount { get; set; }
        public bool HasExistingRelationship { get; set; } = false;

        public List<MentorReviewViewModel> Reviews { get; set; } = new List<MentorReviewViewModel>();

        public string[] SkillsArray =>
            !string.IsNullOrEmpty(Skills) ? Skills.Split(',') : new string[0];
    }

    public class MentorReviewViewModel
    {
        public string MenteeName { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public string Date { get; set; }
    }

    public class EditMentorProfileViewModel
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

        [Required(ErrorMessage = "Expertise is required")]
        [MaxLength(200, ErrorMessage = "Expertise cannot exceed 200 characters")]
        [Display(Name = "Expertise")]
        public string Expertise { get; set; }

        [Required(ErrorMessage = "Skills are required")]
        [MaxLength(500, ErrorMessage = "Skills cannot exceed 500 characters")]
        [Display(Name = "Skills (comma separated)")]
        public string Skills { get; set; }

        [Required(ErrorMessage = "Years of experience is required")]
        [Range(0, 50, ErrorMessage = "Years of experience must be between 0 and 50")]
        [Display(Name = "Years of Experience")]
        public int YearsOfExperience { get; set; }

        [MaxLength(500, ErrorMessage = "Bio cannot exceed 500 characters")]
        [Display(Name = "About Me")]
        public string Bio { get; set; }

        [Required(ErrorMessage = "Availability is required")]
        [MaxLength(50, ErrorMessage = "Availability cannot exceed 50 characters")]
        [Display(Name = "Availability")]
        public string Availability { get; set; } = "";
    }

    public class CreateReviewViewModel
    {
        [Required]
        public int MentorId { get; set; }

        [Required(ErrorMessage = "Rating is required")]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int Rating { get; set; }

        [Required(ErrorMessage = "Comment is required")]
        [MaxLength(500, ErrorMessage = "Comment cannot exceed 500 characters")]
        public string Comment { get; set; }
    }
}