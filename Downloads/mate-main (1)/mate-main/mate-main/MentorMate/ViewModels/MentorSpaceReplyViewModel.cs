using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MentorMate.ViewModels
{
    public class MentorSpaceReplyViewModel
    {
        public int ReplyId { get; set; }
        public string Content { get; set; }
        public UserViewModel CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<MentorSpaceReplyViewModel> Replies { get; set; }
    }

    public class CreateReplyViewModel
    {
        [Required]
        public int PostId { get; set; }

        public int? ParentReplyId { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Content { get; set; }
    }
}