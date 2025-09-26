using System;

namespace MentorMate.ViewModels
{
    public class MentorRequestViewModel
    {
        public int Id { get; set; }
        public string MenteeName { get; set; }
        public string Message { get; set; }
        public DateTime ProposedDate { get; set; }
        public string ProposedTime { get; set; }
        public string SessionType { get; set; }
    }
}