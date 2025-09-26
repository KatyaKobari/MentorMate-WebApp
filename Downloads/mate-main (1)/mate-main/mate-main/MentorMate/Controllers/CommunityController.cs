using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MentorMate.Models;

namespace MentorMate.Controllers
{
    public class CommunityController : Controller
    {
        private readonly AppDbContext _context;

        public CommunityController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> MentorSpace()
        {
            if (!HttpContext.Session.GetInt32("UserId").HasValue)
                return RedirectToAction("Login", "Account");

            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            ViewBag.UserRole = HttpContext.Session.GetString("UserRole");

            var posts = await _context.MentorSpacePosts
                .Include(p => p.CreatedBy)
                .Include(p => p.Replies)
                    .ThenInclude(r => r.CreatedBy)
                .Include(p => p.Replies)
                    .ThenInclude(r => r.ChildReplies)
                        .ThenInclude(cr => cr.CreatedBy)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return View(posts);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePost([FromForm] string title, [FromForm] string content, [FromForm] string type)
        {
            if (!HttpContext.Session.GetInt32("UserId").HasValue)
                return Json(new { success = false, message = "Not authenticated" });

            var userId = HttpContext.Session.GetInt32("UserId").Value;

            var post = new MentorSpacePost
            {
                Title = title,
                Content = content,
                Type = type,
                CreatedById = userId,
                CreatedAt = DateTime.Now
            };

            _context.MentorSpacePosts.Add(post);
            await _context.SaveChangesAsync();

            return Json(new { success = true, postId = post.PostId });
        }

        [HttpPost]
        public async Task<IActionResult> AddComment([FromForm] int postId, [FromForm] string content, [FromForm] int? parentReplyId = null)
        {
            if (!HttpContext.Session.GetInt32("UserId").HasValue)
                return Json(new { success = false, message = "Not authenticated" });

            var userId = HttpContext.Session.GetInt32("UserId").Value;

            var reply = new MentorSpaceReply
            {
                PostId = postId,
                ParentReplyId = parentReplyId,
                CreatedById = userId,
                Content = content,
                CreatedAt = DateTime.Now
            };

            _context.MentorSpaceReplies.Add(reply);
            await _context.SaveChangesAsync();

            return Json(new { success = true, replyId = reply.ReplyId });
        }

        [HttpGet]
        public async Task<IActionResult> GetPosts()
        {
            var posts = await _context.MentorSpacePosts
                .Include(p => p.CreatedBy)
                .Include(p => p.Replies)
                    .ThenInclude(r => r.CreatedBy)
                .Include(p => p.Replies)
                    .ThenInclude(r => r.ChildReplies)
                        .ThenInclude(cr => cr.CreatedBy)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            var result = posts.Select(p => new
            {
                id = p.PostId,
                user = new
                {
                    name = p.CreatedBy.FullName,
                    role = _context.MentorProfiles.Any(m => m.MentorId == p.CreatedById) ? "Mentor" : "Mentee",
                    avatar = $"https://i.pravatar.cc/50?u={p.CreatedBy.UserId}"
                },
                title = p.Title,
                content = p.Content,
                type = p.Type,
                comments = p.Replies.Where(r => r.ParentReplyId == null).Select(r => new
                {
                    name = r.CreatedBy.FullName,
                    text = r.Content,
                    replies = r.ChildReplies.Select(cr => new
                    {
                        name = cr.CreatedBy.FullName,
                        text = cr.Content,
                        replies = new object[0] // يمكن إضافة المزيد من التداخل إذا needed
                    })
                }),
                date = p.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss")
            });

            return Json(result);
        }
    }
}