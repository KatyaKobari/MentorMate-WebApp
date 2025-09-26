using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using MentorMate.Models;
using MentorMate.ViewModels;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace MentorMate.Controllers
{
    public class MentorSpaceController : Controller
    {
        private readonly AppDbContext _context;

        public MentorSpaceController(AppDbContext context)
        {
            _context = context;
        }

        // GET: MentorSpace
        public async Task<IActionResult> Index()
        {
            // إذا ما عندك بيانات، ضيف بيانات تجريبية
            if (!_context.MentorSpacePosts.Any())
            {
                // إضافة بيانات تجريبية
                var demoUser = _context.Users.FirstOrDefault();
                if (demoUser != null)
                {
                    var demoPost = new MentorSpacePost
                    {
                        Title = "How to learn ASP.NET Core?",
                        Content = "I'm new to ASP.NET Core and looking for the best resources to learn. Any advice?",
                        Type = "Question",
                        CreatedById = demoUser.UserId,
                        CreatedAt = DateTime.Now
                    };
                    _context.MentorSpacePosts.Add(demoPost);
                    await _context.SaveChangesAsync();
                }
            }

            var posts = await _context.MentorSpacePosts
                .Include(p => p.CreatedBy)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.CreatedBy)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            var viewModel = posts.Select(p => new MentorSpacePostViewModel
            {
                PostId = p.PostId,
                Title = p.Title,
                Content = p.Content,
                Type = p.Type,
                CreatedBy = new UserViewModel
                {
                    UserId = p.CreatedBy.UserId,
                    FullName = p.CreatedBy.FullName,
                    Role = _context.MentorProfiles.Any(m => m.MentorId == p.CreatedBy.UserId) ? "Mentor" : "Mentee",
                    AvatarUrl = $"https://i.pravatar.cc/50?u={p.CreatedBy.UserId}"
                },
                CreatedAt = p.CreatedAt,
                CommentCount = p.Comments.Count,
                Comments = p.Comments.Where(c => c.ParentReplyId == null)
                    .Select(c => new MentorSpaceReplyViewModel
                    {
                        ReplyId = c.ReplyId,
                        Content = c.Content,
                        CreatedBy = new UserViewModel
                        {
                            UserId = c.CreatedBy.UserId,
                            FullName = c.CreatedBy.FullName,
                            Role = _context.MentorProfiles.Any(m => m.MentorId == c.CreatedBy.UserId) ? "Mentor" : "Mentee",
                            AvatarUrl = $"https://i.pravatar.cc/40?u={c.CreatedBy.UserId}"
                        },
                        CreatedAt = c.CreatedAt,
                        Replies = new List<MentorSpaceReplyViewModel>()
                    }).ToList()
            }).ToList();

            return View(viewModel);
        }

        // باقي الـ Actions...
    }
}