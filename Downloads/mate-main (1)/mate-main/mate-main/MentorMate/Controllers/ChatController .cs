using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using MentorMate.Models;
using System;
using System.Collections.Generic;

namespace MentorMate.Controllers
{
    public class ChatController : Controller
    {
        private readonly AppDbContext _context;

        public ChatController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            if (!HttpContext.Session.GetInt32("UserId").HasValue)
                return RedirectToAction("Login", "Account");

            var userId = HttpContext.Session.GetInt32("UserId").Value;
            ViewBag.UserId = userId;
            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            ViewBag.UserRole = HttpContext.Session.GetString("UserRole");

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetChatUsers()
        {
            if (!HttpContext.Session.GetInt32("UserId").HasValue)
                return Json(new { success = false, message = "Not authenticated" });

            var currentUserId = HttpContext.Session.GetInt32("UserId").Value;

            try
            {
                // Get users who have approved mentorship relationships with the current user
                var approvedRelationships = await _context.MentorshipRequests
                    .Where(r => r.Status == "Approved" && 
                               (r.MentorId == currentUserId || r.MenteeId == currentUserId))
                    .Select(r => new { 
                        MentorId = r.MentorId, 
                        MenteeId = r.MenteeId 
                    })
                    .ToListAsync();

                var connectedUserIds = approvedRelationships
                    .SelectMany(r => new[] { r.MentorId, r.MenteeId })
                    .Where(id => id != currentUserId)
                    .Distinct()
                    .ToList();

                // Get users who have had conversations with the current user
                var usersWithMessages = await _context.Messages
                    .Where(m => (m.SenderId == currentUserId || m.ReceiverId == currentUserId) &&
                               connectedUserIds.Contains(m.SenderId == currentUserId ? m.ReceiverId : m.SenderId))
                    .Select(m => m.SenderId == currentUserId ? m.ReceiverId : m.SenderId)
                    .Distinct()
                    .ToListAsync();

                var users = await _context.Users
                    .Where(u => connectedUserIds.Contains(u.UserId))
                    .Select(u => new
                    {
                        userId = u.UserId,
                        fullName = u.FullName,
                        role = u.MentorProfile != null ? "Mentor" : "Mentee",
                        avatarUrl = u.Gender == "Female" ? 
                            "data:image/svg+xml;base64," + Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"<svg xmlns='http://www.w3.org/2000/svg' width='100' height='100' viewBox='0 0 100 100'><circle cx='50' cy='50' r='50' fill='#e91e63'/><text x='50' y='60' text-anchor='middle' fill='white' font-size='40' font-family='Arial'>♀</text></svg>")) :
                            "data:image/svg+xml;base64," + Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"<svg xmlns='http://www.w3.org/2000/svg' width='100' height='100' viewBox='0 0 100 100'><circle cx='50' cy='50' r='50' fill='#2196f3'/><text x='50' y='60' text-anchor='middle' fill='white' font-size='40' font-family='Arial'>♂</text></svg>")),
                        lastMessage = _context.Messages
                            .Where(m => (m.SenderId == currentUserId && m.ReceiverId == u.UserId) ||
                                       (m.SenderId == u.UserId && m.ReceiverId == currentUserId))
                            .OrderByDescending(m => m.SentAt)
                            .Select(m => new
                            {
                                content = m.Content,
                                sentAt = m.SentAt,
                                isRead = m.IsRead,
                                senderId = m.SenderId
                            })
                            .FirstOrDefault(),
                        unreadCount = _context.Messages
                            .Count(m => m.SenderId == u.UserId && m.ReceiverId == currentUserId && !m.IsRead)
                    })
                    .OrderByDescending(u => u.lastMessage != null ? u.lastMessage.sentAt : DateTime.MinValue)
                    .ToListAsync();

                return Json(new { success = true, data = users });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred while loading chat users" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetMessages(int otherUserId, DateTime? before = null)
        {
            if (!HttpContext.Session.GetInt32("UserId").HasValue)
                return Json(new { success = false, message = "Not authenticated" });

            var currentUserId = HttpContext.Session.GetInt32("UserId").Value;

            try
            {
                var query = _context.Messages
                    .Where(m => (m.SenderId == currentUserId && m.ReceiverId == otherUserId) ||
                               (m.SenderId == otherUserId && m.ReceiverId == currentUserId));

                if (before.HasValue)
                {
                    query = query.Where(m => m.SentAt < before.Value);
                }

                var messages = await query
                    .OrderByDescending(m => m.SentAt)
                    .Take(50)
                    .Select(m => new
                    {
                        m.MessageId,
                        m.SenderId,
                        m.ReceiverId,
                        m.Content,
                        m.SentAt,
                        m.IsRead,
                        SenderName = m.Sender.FullName
                    })
                    .OrderBy(m => m.SentAt)
                    .ToListAsync();

                // تحديث الرسائل كمقروءة
                var unreadMessages = await _context.Messages
                    .Where(m => m.SenderId == otherUserId && m.ReceiverId == currentUserId && !m.IsRead)
                    .ToListAsync();

                if (unreadMessages.Any())
                {
                    foreach (var message in unreadMessages)
                    {
                        message.IsRead = true;
                        message.ReadAt = DateTime.Now;
                    }
                    await _context.SaveChangesAsync();
                }

                return Json(new { success = true, data = messages });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
        {
            if (!HttpContext.Session.GetInt32("UserId").HasValue)
                return Json(new { success = false, message = "Not authenticated" });

            if (string.IsNullOrWhiteSpace(request.Content))
                return Json(new { success = false, message = "Message content is required" });

            if (request.ReceiverId <= 0)
                return Json(new { success = false, message = "Invalid receiver ID" });

            var currentUserId = HttpContext.Session.GetInt32("UserId").Value;

            // Check if receiver exists
            var receiverExists = await _context.Users.AnyAsync(u => u.UserId == request.ReceiverId);
            if (!receiverExists)
                return Json(new { success = false, message = "Receiver not found" });

            // Check if there's an approved mentorship relationship
            var hasApprovedRelationship = await _context.MentorshipRequests
                .AnyAsync(r => r.Status == "Approved" && 
                              ((r.MentorId == currentUserId && r.MenteeId == request.ReceiverId) ||
                               (r.MentorId == request.ReceiverId && r.MenteeId == currentUserId)));

            if (!hasApprovedRelationship)
                return Json(new { success = false, message = "You can only chat with users who have approved mentorship relationships with you" });

            try
            {
                var message = new Message
                {
                    SenderId = currentUserId,
                    ReceiverId = request.ReceiverId,
                    Content = request.Content.Trim(),
                    SentAt = DateTime.Now,
                    IsRead = false
                };

                _context.Messages.Add(message);
                await _context.SaveChangesAsync();

                // جلب البيانات المحدثة مع اسم المرسل
                var messageWithSender = await _context.Messages
                    .Where(m => m.MessageId == message.MessageId)
                    .Select(m => new
                    {
                        m.MessageId,
                        m.SenderId,
                        m.ReceiverId,
                        m.Content,
                        m.SentAt,
                        m.IsRead,
                        SenderName = m.Sender.FullName
                    })
                    .FirstOrDefaultAsync();

                return Json(new
                {
                    success = true,
                    message = "Message sent successfully",
                    data = messageWithSender
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred while sending the message" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int messageId)
        {
            if (!HttpContext.Session.GetInt32("UserId").HasValue)
                return Json(new { success = false, message = "Not authenticated" });

            try
            {
                var message = await _context.Messages.FindAsync(messageId);
                if (message == null)
                    return Json(new { success = false, message = "Message not found" });

                if (message.ReceiverId != HttpContext.Session.GetInt32("UserId").Value)
                    return Json(new { success = false, message = "Unauthorized" });

                message.IsRead = true;
                message.ReadAt = DateTime.Now;
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Message marked as read" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetUnreadCount()
        {
            if (!HttpContext.Session.GetInt32("UserId").HasValue)
                return Json(new { success = false, message = "Not authenticated" });

            var currentUserId = HttpContext.Session.GetInt32("UserId").Value;

            try
            {
                var unreadCount = await _context.Messages
                    .CountAsync(m => m.ReceiverId == currentUserId && !m.IsRead);

                return Json(new { success = true, data = unreadCount });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteMessage([FromBody] DeleteMessageRequest request)
        {
            if (!HttpContext.Session.GetInt32("UserId").HasValue)
                return Json(new { success = false, message = "Not authenticated" });

            try
            {
                var message = await _context.Messages.FindAsync(request.MessageId);
                if (message == null)
                    return Json(new { success = false, message = "Message not found" });

                if (message.SenderId != HttpContext.Session.GetInt32("UserId").Value)
                    return Json(new { success = false, message = "Unauthorized" });

                _context.Messages.Remove(message);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Message deleted successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteChat([FromBody] DeleteChatRequest request)
        {
            if (!HttpContext.Session.GetInt32("UserId").HasValue)
                return Json(new { success = false, message = "Not authenticated" });

            var currentUserId = HttpContext.Session.GetInt32("UserId").Value;

            try
            {
                // Get all messages between current user and the other user
                var messages = await _context.Messages
                    .Where(m => (m.SenderId == currentUserId && m.ReceiverId == request.OtherUserId) ||
                               (m.SenderId == request.OtherUserId && m.ReceiverId == currentUserId))
                    .ToListAsync();

                if (messages.Any())
                {
                    _context.Messages.RemoveRange(messages);
                    await _context.SaveChangesAsync();
                }

                return Json(new { success = true, message = "Chat deleted successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            if (!HttpContext.Session.GetInt32("UserId").HasValue)
                return Json(new { success = false, message = "Not authenticated" });

            var currentUserId = HttpContext.Session.GetInt32("UserId").Value;

            try
            {
                var users = await _context.Users
                    .Where(u => u.UserId != currentUserId)
                    .Select(u => new
                    {
                        userId = u.UserId,
                        fullName = u.FullName,
                        role = u.MentorProfile != null ? "Mentor" : "Mentee",
                    avatarUrl = u.Gender == "Female" ? 
                        "data:image/svg+xml;base64," + Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"<svg xmlns='http://www.w3.org/2000/svg' width='100' height='100' viewBox='0 0 100 100'><circle cx='50' cy='50' r='50' fill='#e91e63'/><text x='50' y='60' text-anchor='middle' fill='white' font-size='40' font-family='Arial'>♀</text></svg>")) :
                        "data:image/svg+xml;base64," + Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"<svg xmlns='http://www.w3.org/2000/svg' width='100' height='100' viewBox='0 0 100 100'><circle cx='50' cy='50' r='50' fill='#2196f3'/><text x='50' y='60' text-anchor='middle' fill='white' font-size='40' font-family='Arial'>♂</text></svg>"))
                    })
                    .OrderBy(u => u.fullName)
                    .ToListAsync();

                return Json(new { success = true, data = users });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred while loading users" });
            }
        }
    }

    public class SendMessageRequest
    {
        public int ReceiverId { get; set; }
        public string Content { get; set; }
    }

    public class DeleteMessageRequest
    {
        public int MessageId { get; set; }
    }

    public class DeleteChatRequest
    {
        public int OtherUserId { get; set; }
    }
}