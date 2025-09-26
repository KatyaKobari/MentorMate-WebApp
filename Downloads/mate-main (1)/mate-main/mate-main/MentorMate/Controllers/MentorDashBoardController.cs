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
    public class MentorDashBoardController : Controller
    {
        private readonly AppDbContext _context;

        public MentorDashBoardController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            if (!HttpContext.Session.GetInt32("UserId").HasValue)
                return RedirectToAction("Login", "Account");

            var userId = HttpContext.Session.GetInt32("UserId").Value;
            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            ViewBag.UserRole = HttpContext.Session.GetString("UserRole");
            
            // Pass TempData messages to view
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            ViewBag.ErrorMessage = TempData["ErrorMessage"];

            // التحقق من أن المستخدم هو منتور (من الـ session أولاً)
            var sessionRole = HttpContext.Session.GetString("UserRole");
            var isMentor = sessionRole == "Mentor";

            if (!isMentor)
            {
                // إذا لم يكن منتور في الـ session، تحقق من قاعدة البيانات
                isMentor = await _context.MentorProfiles.AnyAsync(m => m.MentorId == userId);
                if (isMentor)
                {
                    // تحديث الـ session
                    HttpContext.Session.SetString("UserRole", "Mentor");
                }
                else
                {
                    // إذا لم يكن لديه ملف منتور، أنشئ واحد
                    var mentorProfile = new MentorProfile
                    {
                        MentorId = userId,
                        Expertise = "Not specified",
                        Skills = "Not specified",
                        YearsOfExperience = 0,
                        Availability = "Available",
                        Bio = "",
                        Rating = 0,
                        ReviewCount = 0
                    };
                    _context.MentorProfiles.Add(mentorProfile);
                    await _context.SaveChangesAsync();
                    
                    // تحديث الـ session
                    HttpContext.Session.SetString("UserRole", "Mentor");
                }
            }

            // جلب الطلبات الواردة - التصحيح هنا
            var requests = await _context.MentorshipRequests
                .Include(r => r.Mentee) // الآن Mentee هو User مباشرة
                .Where(r => r.MentorId == userId && r.Status == "Pending")
                .Select(r => new MentorRequestViewModel
                {
                    Id = r.RequestId,
                    MenteeName = r.Mentee.FullName, // التصحيح: r.Mentee.FullName مباشرة
                    Message = r.Message,
                    ProposedDate = r.ProposedDate,
                    ProposedTime = r.ProposedTime,
                    SessionType = r.SessionType
                })
                .ToListAsync();

            // جلب المنتيز المتصلين - التصحيح هنا
            var connections = await _context.MentorshipRequests
                .Include(r => r.Mentee)
                .ThenInclude(m => m.MenteeProfile)
                .Where(r => r.MentorId == userId && r.Status == "Approved")
                .Select(r => new MenteeConnectionViewModel
                {
                    MenteeId = r.MenteeId,
                    MenteeName = r.Mentee.FullName,
                    FieldOfStudy = r.Mentee.MenteeProfile != null ? r.Mentee.MenteeProfile.FieldOfStudy : "Not specified",
                    ConnectedSince = r.CreatedAt,
                    Gender = r.Mentee.Gender
                })
                .ToListAsync();

            // جلب الإشعارات
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .OrderByDescending(n => n.CreatedAt)
                .Take(10)
                .ToListAsync();

            var model = new MentorDashboardViewModel
            {
                Requests = requests,
                Connections = connections,
                Notifications = notifications,
                UnreadNotificationCount = notifications.Count
            };

            return View("~/Views/Mentor/Dashboard.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> AcceptRequest([FromBody] AcceptRejectRequestModel model)
        {
            try
            {
                if (!HttpContext.Session.GetInt32("UserId").HasValue)
                    return Json(new { success = false, message = "Not authenticated" });

                var userId = HttpContext.Session.GetInt32("UserId").Value;
                var request = await _context.MentorshipRequests
                    .Include(r => r.Mentee)
                    .FirstOrDefaultAsync(r => r.RequestId == model.requestId && r.MentorId == userId);

                if (request == null)
                    return Json(new { success = false, message = "Request not found" });

                request.Status = "Approved";
                request.UpdatedAt = DateTime.Now;

                // إضافة إشعار للمنتي
                var notification = new Notification
                {
                    UserId = request.MenteeId,
                    Message = $"Your session request with {HttpContext.Session.GetString("UserName")} has been approved!",
                    Type = "RequestApproved",
                    IsRead = false,
                    CreatedAt = DateTime.Now
                };
                _context.Notifications.Add(notification);

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Request accepted successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred while processing the request" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> RejectRequest([FromBody] AcceptRejectRequestModel model)
        {
            try
            {
                if (!HttpContext.Session.GetInt32("UserId").HasValue)
                    return Json(new { success = false, message = "Not authenticated" });

                var userId = HttpContext.Session.GetInt32("UserId").Value;
                var request = await _context.MentorshipRequests
                    .Include(r => r.Mentee)
                    .FirstOrDefaultAsync(r => r.RequestId == model.requestId && r.MentorId == userId);

                if (request == null)
                    return Json(new { success = false, message = "Request not found" });

                request.Status = "Rejected";
                request.UpdatedAt = DateTime.Now;

                // إضافة إشعار للمنتي
                var notification = new Notification
                {
                    UserId = request.MenteeId,
                    Message = $"Your session request with {HttpContext.Session.GetString("UserName")} has been declined.",
                    Type = "RequestRejected",
                    IsRead = false,
                    CreatedAt = DateTime.Now
                };
                _context.Notifications.Add(notification);

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Request rejected successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred while processing the request" });
            }
        }

        // باقي الدوال تبقى كما هي...
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