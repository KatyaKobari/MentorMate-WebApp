using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MentorMate.Models;
using MentorMate.ViewModels;

namespace MentorMate.Controllers
{
    public class MentorshipController : Controller
    {
        private readonly AppDbContext _context;

        public MentorshipController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Mentorship/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Mentorship/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MentorshipRequestViewModel model)
        {
            try
            {
                int? menteeId = HttpContext.Session.GetInt32("UserId");
                if (menteeId == null)
                    return RedirectToAction("Login", "Account");

                // التحقق من وجود المينتور
                var mentor = await _context.Users
                    .Include(u => u.MentorProfile)
                    .FirstOrDefaultAsync(u => u.UserId == model.MentorId && u.MentorProfile != null);

                if (mentor == null)
                {
                    ModelState.AddModelError("", "Mentor not found.");
                    return View(model);
                }

                // التحقق من عدم وجود طلب سابق
                var existingRequest = await _context.MentorshipRequests
                    .FirstOrDefaultAsync(r => r.MentorId == model.MentorId && r.MenteeId == menteeId.Value);

                if (existingRequest != null)
                {
                    ModelState.AddModelError("", "You have already sent a request to this mentor.");
                    return View(model);
                }

                // إنشاء طلب Mentorship جديد
                var request = new MentorshipRequest
                {
                    MentorId = model.MentorId,
                    MenteeId = menteeId.Value,
                    Status = "Pending",
                    CreatedAt = DateTime.Now
                };

                _context.MentorshipRequests.Add(request);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index", "Profile");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while creating the request. Please try again.");
                return View(model);
            }
        }
    }
}
