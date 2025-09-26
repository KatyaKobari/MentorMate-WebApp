using System.ComponentModel.DataAnnotations;

namespace MentorMate.ViewModels
{
    public class UserViewModel
    {
        public int UserId { get; set; }

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; }

        [Required]
        [MaxLength(10)]
        public string Role { get; set; } // "Mentor" or "Mentee"

        public string AvatarUrl { get; set; }
    }
}