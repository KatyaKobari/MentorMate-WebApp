using System;

namespace MentorMate.ViewModels
{
    public class MentorshipRequestViewModel
    {
        public int Id { get; set; }
        public string MentorName { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
        public DateTime CreatedAt { get; set; }
        public int MentorId { get; set; }
    }
}