using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MentorMate.Models
{
    public class MentorshipRequest
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RequestId { get; set; }

        [Required]
        [ForeignKey("Mentee")]
        public int MenteeId { get; set; }

        [Required]
        [ForeignKey("Mentor")]
        public int MentorId { get; set; }

        public DateTime ProposedDate { get; set; }
        public string ProposedTime { get; set; }
        public string SessionType { get; set; }
        public string Message { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual User Mentee { get; set; }
        public virtual User Mentor { get; set; }
        public virtual ICollection<Chat> Chats { get; set; } = new List<Chat>();
    }
}