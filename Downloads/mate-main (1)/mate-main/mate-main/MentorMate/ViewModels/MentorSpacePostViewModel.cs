using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MentorMate.ViewModels
{
    public class MentorSpacePostViewModel
    {
        public int PostId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Type { get; set; }
        public UserViewModel CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public int CommentCount { get; set; }
        public List<MentorSpaceReplyViewModel> Comments { get; set; }
    }

    public class CreatePostViewModel
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        [Required]
        [MaxLength(2000)]
        public string Content { get; set; }

        [Required]
        public string Type { get; set; } // Question, Advice, Experience
    }
}