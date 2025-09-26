using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MentorMate.Models;
using MentorMate.ViewModels;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Threading.Tasks;

namespace MentorMate.Controllers
{
    public class MenteeController : Controller
    {
        private readonly AppDbContext _context;

        public MenteeController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Mentee/Profile
        public async Task<IActionResult> Profile(int? id = null)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // If id is provided, view that mentee's profile, otherwise view current user's profile
            var targetUserId = id ?? userId.Value;

            var menteeProfile = await _context.MenteeProfiles
                .Include(m => m.User)
                .FirstOrDefaultAsync(m => m.MenteeId == targetUserId);

            if (menteeProfile == null)
            {
                menteeProfile = new MenteeProfile
                {
                    MenteeId = userId.Value,
                    User = await _context.Users.FindAsync(userId)
                };
                _context.MenteeProfiles.Add(menteeProfile);
                await _context.SaveChangesAsync();
            }

            var viewModel = new MenteeProfileViewModel
            {
                MenteeId = menteeProfile.MenteeId,
                FullName = menteeProfile.User.FullName,
                Email = menteeProfile.User.Email,
                Gender = menteeProfile.User.Gender,
                LinkedInUrl = menteeProfile.User.LinkedInUrl,
                FieldOfStudy = menteeProfile.FieldOfStudy,
                Bio = menteeProfile.Bio,
                Interests = menteeProfile.Interests,
                AvatarUrl = $"https://i.pravatar.cc/150?u={menteeProfile.MenteeId}",
                SessionCount = 0 // قيمة ثابتة لأنو ما عندك جلسات
            };

            return View(viewModel);
        }

        // GET: Mentee/EditProfile
        public async Task<IActionResult> EditProfile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var menteeProfile = await _context.MenteeProfiles
                .Include(m => m.User)
                .FirstOrDefaultAsync(m => m.MenteeId == userId);

            var viewModel = new EditMenteeProfileViewModel
            {
                FullName = menteeProfile?.User?.FullName ?? "",
                Email = menteeProfile?.User?.Email ?? "",
                Gender = menteeProfile?.User?.Gender ?? "Male",
                LinkedInUrl = menteeProfile?.User?.LinkedInUrl ?? "",
                FieldOfStudy = menteeProfile?.FieldOfStudy,
                Bio = menteeProfile?.Bio,
                Interests = menteeProfile?.Interests
            };

            return View(viewModel);
        }

        // POST: Mentee/EditProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(EditMenteeProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var menteeProfile = await _context.MenteeProfiles
                .Include(m => m.User)
                .FirstOrDefaultAsync(m => m.MenteeId == userId);

            if (menteeProfile == null)
            {
                menteeProfile = new MenteeProfile { MenteeId = userId.Value };
                _context.MenteeProfiles.Add(menteeProfile);
            }

            // Update User table
            if (menteeProfile.User != null)
            {
                menteeProfile.User.FullName = model.FullName;
                menteeProfile.User.Email = model.Email;
                menteeProfile.User.Gender = model.Gender;
                menteeProfile.User.LinkedInUrl = model.LinkedInUrl;
            }

            // Update MenteeProfile table
            menteeProfile.FieldOfStudy = model.FieldOfStudy;
            menteeProfile.Bio = model.Bio;
            menteeProfile.Interests = model.Interests;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Profile updated successfully!";
            return RedirectToAction("Profile");
        }

        // GET: Mentee/Dashboard
        public IActionResult Dashboard()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }
    }
}