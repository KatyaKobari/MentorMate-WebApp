using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MentorMate.Models
{
    public class MenteeProfile
    {
        [Key]
        [ForeignKey("User")]
        public int MenteeId { get; set; }

        [MaxLength(500)]
        public string Bio { get; set; }

        [MaxLength(200)]
        public string FieldOfStudy { get; set; }

        [MaxLength(300)]
        public string Interests { get; set; }

        [MaxLength(500)]
        public string Goals { get; set; }

        public virtual User User { get; set; }
    }
}