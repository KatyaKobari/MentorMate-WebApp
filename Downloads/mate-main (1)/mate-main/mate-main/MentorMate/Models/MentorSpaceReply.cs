using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MentorMate.Models
{
    public class MentorSpaceReply
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ReplyId { get; set; }

        [ForeignKey("Post")]
        public int PostId { get; set; }
        public virtual MentorSpacePost Post { get; set; }

        [ForeignKey("ParentReply")]
        public int? ParentReplyId { get; set; } // إضافة للردود المتداخلة
        public virtual MentorSpaceReply ParentReply { get; set; }

        [ForeignKey("CreatedBy")]
        public int CreatedById { get; set; }
        public virtual User CreatedBy { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Content { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        // الردود الفرعية
        public virtual ICollection<MentorSpaceReply> ChildReplies { get; set; } = new List<MentorSpaceReply>();
    }
}