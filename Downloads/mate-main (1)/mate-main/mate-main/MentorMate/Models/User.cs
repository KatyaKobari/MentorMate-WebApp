using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MentorMate.Models
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; }

        [Required]
        [MaxLength(150)]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        [MaxLength(500)]
        public string Bio { get; set; }

        [MaxLength(200)]
        public string LinkedInUrl { get; set; }

        [Required]
        [MaxLength(10)]
        public string Gender { get; set; } = "Male"; // Default to Male

        // علاقات MentorshipRequest
        public virtual ICollection<MentorshipRequest> MentorRequests { get; set; } = new List<MentorshipRequest>();
        public virtual ICollection<MentorshipRequest> MenteeRequests { get; set; } = new List<MentorshipRequest>();

        // علاقات Chat
        public virtual ICollection<Chat> Chats { get; set; } = new List<Chat>();

        // علاقات Message
        public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
        public virtual ICollection<Message> ReceivedMessages { get; set; } = new List<Message>();

        // علاقات MentorSpace
        public virtual ICollection<MentorSpacePost> MentorPosts { get; set; } = new List<MentorSpacePost>();
        public virtual ICollection<MentorSpaceReply> MentorReplies { get; set; } = new List<MentorSpaceReply>();

        // علاقات الإشعارات
        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

        // علاقات الملفات الشخصية
        public virtual MentorProfile MentorProfile { get; set; }
        public virtual MenteeProfile MenteeProfile { get; set; }
    }
}