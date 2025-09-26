using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MentorMate.Models
{
    public class MentorSpacePost
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PostId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } // إضافة حقل العنوان

        [Required]
        [MaxLength(2000)]
        public string Content { get; set; }

        [Required]
        [MaxLength(20)]
        public string Type { get; set; } = "Question"; // Question, Advice, Experience

        [ForeignKey("CreatedBy")]
        public int CreatedById { get; set; }
        public virtual User CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        public virtual ICollection<MentorSpaceReply> Replies { get; set; } = new List<MentorSpaceReply>();

        // إضافة خاصية التنقل العكسي
        public virtual ICollection<MentorSpaceReply> Comments { get; set; } = new List<MentorSpaceReply>();
    }
}