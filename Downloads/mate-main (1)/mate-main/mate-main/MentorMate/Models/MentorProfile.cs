using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MentorMate.Models
{
    public class MentorProfile
    {
        [Key]
        [ForeignKey("User")]
        public int MentorId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Expertise { get; set; }

        [MaxLength(500)]
        public string Skills { get; set; }

        public int YearsOfExperience { get; set; }

        [MaxLength(50)]
        public string Availability { get; set; } = "Available";

        [MaxLength(500)]
        public string Bio { get; set; }

        [Column(TypeName = "decimal(3,1)")]
        public decimal Rating { get; set; } = 0;

        public int ReviewCount { get; set; } = 0;

        public virtual User User { get; set; }
        public ICollection<MentorshipRequest> MentorRequests { get; set; }

    }
}