using System;

namespace MentorMate.ViewModels
{
    public class MenteeConnectionViewModel
    {
        public int MenteeId { get; set; }
        public string MenteeName { get; set; }
        public string FieldOfStudy { get; set; }
        public DateTime ConnectedSince { get; set; }
        public string Gender { get; set; } = "Male";
    }
}