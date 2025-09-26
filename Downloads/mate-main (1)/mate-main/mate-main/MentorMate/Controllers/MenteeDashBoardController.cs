using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using MentorMate.Models;
using MentorMate.ViewModels;
using System;

namespace MentorMate.Controllers
{
    public class MenteeDashboardController : Controller
    {
        private readonly AppDbContext _context;

        public MenteeDashboardController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string search, string expertise, int minYears = 0, string sortBy = "featured", int? requestMentorId = null)
        {
            if (!HttpContext.Session.GetInt32("UserId").HasValue)
                return RedirectToAction("Login", "Account");

            var userId = HttpContext.Session.GetInt32("UserId").Value;
            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            ViewBag.UserRole = HttpContext.Session.GetString("UserRole");
            ViewBag.RequestMentorId = requestMentorId;
            
            // Pass TempData messages to view
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            ViewBag.ErrorMessage = TempData["ErrorMessage"];

            // التحقق من أن المستخدم هو منتي (من الـ session أولاً)
            var sessionRole = HttpContext.Session.GetString("UserRole");
            var isMentee = sessionRole == "Mentee";

            if (!isMentee)
            {
                // إذا لم يكن منتي في الـ session، تحقق من قاعدة البيانات
                isMentee = await _context.MenteeProfiles.AnyAsync(m => m.MenteeId == userId);
                if (isMentee)
                {
                    // تحديث الـ session
                    HttpContext.Session.SetString("UserRole", "Mentee");
                }
                else
                {
                    // إذا لم يكن لديه ملف منتي، أنشئ واحد
                    var menteeProfile = new MenteeProfile
                    {
                        MenteeId = userId,
                        Bio = "Not specified",
                        FieldOfStudy = "Not specified",
                        Interests = "Not specified",
                        Goals = "Not specified"
                    };
                    _context.MenteeProfiles.Add(menteeProfile);
                    await _context.SaveChangesAsync();
                    
                    // تحديث الـ session
                    HttpContext.Session.SetString("UserRole", "Mentee");
                }
            }

            // بناء الاستعلام الأساسي - استبعاد المستخدم الحالي
            var mentorsQuery = _context.MentorProfiles
                .Include(m => m.User)
                .Where(m => m.MentorId != userId) // استبعاد المستخدم الحالي
                .AsQueryable();

            // تطبيق الفلاتر
            if (!string.IsNullOrEmpty(search))
            {
                var searchLower = search.ToLower();
                mentorsQuery = mentorsQuery.Where(m =>
                    m.User.FullName.ToLower().Contains(searchLower) ||
                    m.Expertise.ToLower().Contains(searchLower) ||
                    m.Skills.ToLower().Contains(searchLower) ||
                    m.Bio.ToLower().Contains(searchLower));
            }

            if (!string.IsNullOrEmpty(expertise))
            {
                mentorsQuery = mentorsQuery.Where(m => m.Expertise.Contains(expertise) || expertise.Contains(m.Expertise));
            }

            if (minYears > 0)
            {
                mentorsQuery = mentorsQuery.Where(m => m.YearsOfExperience >= minYears);
            }

            // تطبيق الترتيب
            switch (sortBy)
            {
                case "rating":
                    mentorsQuery = mentorsQuery.OrderByDescending(m => m.Rating);
                    break;
                case "experience":
                    mentorsQuery = mentorsQuery.OrderByDescending(m => m.YearsOfExperience);
                    break;
                case "name":
                    mentorsQuery = mentorsQuery.OrderBy(m => m.User.FullName);
                    break;
                default: // featured
                    mentorsQuery = mentorsQuery.OrderByDescending(m => m.Rating).ThenByDescending(m => m.YearsOfExperience);
                    break;
            }

            // تنفيذ الاستعلام
            var mentors = await mentorsQuery
                .Select(m => new MentorCardViewModel
                {
                    Id = m.MentorId,
                    Name = m.User.FullName,
                    Expertise = m.Expertise,
                    Years = (int)m.YearsOfExperience,
                    Rating = (double)m.Rating,
                    Skills = m.Skills,
                    Bio = m.Bio,
                    Gender = m.User.Gender
                })
                .ToListAsync();

            // التحقق من العلاقات الموجودة
            var existingRelationships = await _context.MentorshipRequests
                .Where(r => r.MenteeId == userId && r.Status == "Approved")
                .Select(r => r.MentorId)
                .ToListAsync();

            // تحديث حالة العلاقات الموجودة
            foreach (var mentor in mentors)
            {
                mentor.HasExistingRelationship = existingRelationships.Contains(mentor.Id);
            }

            // الحصول على قائمة التخصصات الفريدة من قاعدة البيانات
            var expertiseList = await _context.MentorProfiles
                .Select(m => m.Expertise)
                .Where(e => !string.IsNullOrEmpty(e))
                .Distinct()
                .OrderBy(e => e)
                .ToListAsync();

            // إضافة تخصصات شائعة إذا لم تكن موجودة
            var commonExpertise = new List<string>
            {
                "Data Science", "Web Development", "Machine Learning", "Artificial Intelligence",
                "Mobile Development", "UI/UX Design", "Software Engineering", "DevOps",
                "Cloud Computing", "Cybersecurity", "Database Management", "Project Management",
                "Business Analysis", "Digital Marketing", "Product Management", "Quality Assurance"
            };

            // دمج القوائم وإزالة التكرار
            var allExpertise = expertiseList.Union(commonExpertise).OrderBy(e => e).ToList();

            // الحصول على عدد الطلبات المعلقة
            var pendingRequests = await _context.MentorshipRequests
                .Where(r => r.MenteeId == userId && r.Status == "Pending")
                .CountAsync();

            // جلب الإشعارات
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .OrderByDescending(n => n.CreatedAt)
                .Take(10)
                .ToListAsync();

            var model = new MenteeDashboardViewModel
            {
                Mentors = mentors,
                PendingRequests = pendingRequests,
                Notifications = notifications,
                UnreadNotificationCount = notifications.Count,
                SearchTerm = search,
                SelectedExpertise = expertise,
                SelectedMinYears = minYears,
                SelectedSortBy = sortBy
            };

            ViewBag.ExpertiseList = allExpertise;

            return View("~/Views/Mentee/Dashboard.cshtml", model);
        }

        public IActionResult Profile()
        {
            if (!HttpContext.Session.GetInt32("UserId").HasValue)
                return RedirectToAction("Login", "Account");

            return View();
        }

        public IActionResult Settings()
        {
            if (!HttpContext.Session.GetInt32("UserId").HasValue)
                return RedirectToAction("Login", "Account");

            return View();
        }

        public async Task<IActionResult> Requests()
        {
            if (!HttpContext.Session.GetInt32("UserId").HasValue)
                return RedirectToAction("Login", "Account");

            var menteeId = HttpContext.Session.GetInt32("UserId").Value;

            var requests = await _context.MentorshipRequests
                .Include(r => r.Mentor) // التصحيح: فقط Include للمentor
                .Where(r => r.MenteeId == menteeId)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new MentorshipRequestViewModel
                {
                    Id = r.RequestId,
                    MentorName = r.Mentor.FullName, // التصحيح: r.Mentor.FullName مباشرة
                    Date = r.ProposedDate.ToString("yyyy-MM-dd"),
                    Time = r.ProposedTime,
                    Type = r.SessionType,
                    Status = r.Status,
                    Message = r.Message,
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync();

            return View(requests);
        }

        [HttpPost]
        public async Task<IActionResult> RequestSession([FromBody] RequestSessionModel model)
        {
            try
            {
                if (!HttpContext.Session.GetInt32("UserId").HasValue)
                    return Json(new { success = false, message = "Not authenticated" });

                var menteeId = HttpContext.Session.GetInt32("UserId").Value;

                // Check if mentor exists and is not the current user
                var mentorExists = await _context.MentorProfiles
                    .AnyAsync(m => m.MentorId == model.MentorId && m.MentorId != menteeId);

                if (!mentorExists)
                    return Json(new { success = false, message = "Mentor not found or cannot request yourself" });

                var request = new MentorshipRequest
                {
                    MenteeId = menteeId,
                    MentorId = model.MentorId,
                    ProposedDate = DateTime.Parse(model.Date),
                    ProposedTime = model.Time,
                    SessionType = model.Type,
                    Message = model.Message,
                    Status = "Pending",
                    CreatedAt = DateTime.Now
                };

                _context.MentorshipRequests.Add(request);

                // Add notification for mentor
                var notification = new Notification
                {
                    UserId = model.MentorId,
                    Message = $"You have a new session request from {HttpContext.Session.GetString("UserName")}",
                    Type = "NewRequest",
                    IsRead = false,
                    CreatedAt = DateTime.Now
                };
                _context.Notifications.Add(notification);

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Session request sent successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred while sending the request" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetMentorRequests()
        {
            if (!HttpContext.Session.GetInt32("UserId").HasValue)
                return Json(new { success = false, message = "Not authenticated" });

            var menteeId = HttpContext.Session.GetInt32("UserId").Value;

            var requests = await _context.MentorshipRequests
                .Include(r => r.Mentor) // التصحيح: فقط Include للمentor
                .Where(r => r.MenteeId == menteeId)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new
                {
                    MentorName = r.Mentor.FullName, // التصحيح: r.Mentor.FullName مباشرة
                    Date = r.ProposedDate.ToString("yyyy-MM-dd"),
                    Time = r.ProposedTime,
                    Type = r.SessionType,
                    Status = r.Status,
                    Message = r.Message
                })
                .ToListAsync();

            return Json(new { success = true, data = requests });
        }

        [HttpPost]
        public async Task<IActionResult> MarkNotificationsAsRead()
        {
            if (!HttpContext.Session.GetInt32("UserId").HasValue)
                return Json(new { success = false, message = "Not authenticated" });

            var userId = HttpContext.Session.GetInt32("UserId").Value;

            var unreadNotifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var notification in unreadNotifications)
            {
                notification.IsRead = true;
            }

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Notifications marked as read" });
        }

        [HttpGet]
        public async Task<IActionResult> GetNotifications()
        {
            if (!HttpContext.Session.GetInt32("UserId").HasValue)
                return Json(new { success = false, message = "Not authenticated" });

            var userId = HttpContext.Session.GetInt32("UserId").Value;

            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(10)
                .Select(n => new
                {
                    n.Message,
                    n.CreatedAt,
                    n.IsRead
                })
                .ToListAsync();

            return Json(new { success = true, data = notifications });
        }
    }
}