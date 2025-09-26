using System.ComponentModel.DataAnnotations;

namespace MentorMate.ViewModels
{
    public class RegisterStep2ViewModel
    {
        [Required(ErrorMessage = "Please select a role")]
        [RegularExpression("^(Mentor|Mentee)$", ErrorMessage = "Please select a valid role")]
        [Display(Name = "Role")]
        public string Role { get; set; }
    }
}