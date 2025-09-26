using System;
using System.Collections.Generic;
using MentorMate.Models;
namespace MentorMate.ViewModels
{
    public class MentorDashboardViewModel
    {
        public List<MentorRequestViewModel> Requests { get; set; }
        public List<MenteeConnectionViewModel> Connections { get; set; }
        public List<Notification> Notifications { get; set; }
        public int UnreadNotificationCount { get; set; }
    }
}