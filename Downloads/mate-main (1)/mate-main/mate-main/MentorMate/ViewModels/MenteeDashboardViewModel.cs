using System.Collections.Generic;
using MentorMate.Models;

namespace MentorMate.ViewModels
{
    public class MenteeDashboardViewModel
    {
        public List<MentorCardViewModel> Mentors { get; set; }
        public int PendingRequests { get; set; }
        public List<Notification> Notifications { get; set; }
        public int UnreadNotificationCount { get; set; }

        // إضافة خصائص جديدة لتخزين حالة الفلاتر
        public string SearchTerm { get; set; }
        public string SelectedExpertise { get; set; }
        public int SelectedMinYears { get; set; }
        public string SelectedSortBy { get; set; }
    }
}