using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MentorMate.Models;
using MentorMate.ViewModels;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MentorMate.Controllers
{
    public class MentorController : Controller
    {
        private readonly AppDbContext _context;

        public MentorController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Mentor/Profile
        public async Task<IActionResult> Profile(int? id = null)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // If id is provided, view that mentor's profile, otherwise view current user's profile
            var targetUserId = id ?? userId.Value;

            var mentorProfile = await _context.MentorProfiles
                .Include(m => m.User)
                .FirstOrDefaultAsync(m => m.MentorId == targetUserId);

            if (mentorProfile == null)
            {
                mentorProfile = new MentorProfile
                {
                    MentorId = userId.Value,
                    User = await _context.Users.FindAsync(userId),
                    Expertise = "Software Development",
                    Skills = "C#,ASP.NET,JavaScript",
                    YearsOfExperience = 3,
                    Availability = "Available",
                    Bio = "Passionate mentor with extensive experience",
                    Rating = 4.5m,
                    ReviewCount = 2
                };
                _context.MentorProfiles.Add(mentorProfile);
                await _context.SaveChangesAsync();

                // إضافة تقييمات تجريبية
                var reviews = new[]
                {
                    new MentorReview
                    {
                        MentorId = userId.Value,
                        MenteeId = userId.Value,
                        Rating = 5,
                        Comment = "Great mentor! Helped me understand complex ML topics easily.",
                        CreatedAt = DateTime.Now.AddDays(-10)
                    },
                    new MentorReview
                    {
                        MentorId = userId.Value,
                        MenteeId = userId.Value,
                        Rating = 4,
                        Comment = "Very knowledgeable and patient.",
                        CreatedAt = DateTime.Now.AddDays(-5)
                    }
                };
                _context.MentorReviews.AddRange(reviews);
                await _context.SaveChangesAsync();
            }

            var reviewsList = await _context.MentorReviews
                .Include(r => r.Mentee)
                .Where(r => r.MentorId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .Take(10)
                .Select(r => new MentorReviewViewModel
                {
                    MenteeName = r.Mentee.FullName,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    Date = r.CreatedAt.ToString("MMMM yyyy")
                })
                .ToListAsync();

            // حساب عدد المنتيز - بطريقة آمنة إذا الجدول مش موجود
            int menteeCount = 0;
            try
            {
                menteeCount = await _context.MentorshipRequests
                    .CountAsync(m => m.MentorId == userId && m.Status == "Accepted");
            }
            catch
            {
                menteeCount = 0;
            }

            // Check if there's an existing relationship between current user and this mentor
            var hasExistingRelationship = false;
            if (userId != targetUserId) // Only check if viewing someone else's profile
            {
                hasExistingRelationship = await _context.MentorshipRequests
                    .AnyAsync(r => r.Status == "Approved" && 
                                  ((r.MentorId == targetUserId && r.MenteeId == userId) ||
                                   (r.MentorId == userId && r.MenteeId == targetUserId)));
            }

            // Calculate current rating and review count from database
            var currentReviews = await _context.MentorReviews
                .Where(r => r.MentorId == targetUserId)
                .ToListAsync();
            
            var currentRating = currentReviews.Any() ? (decimal)currentReviews.Average(r => r.Rating) : 0;
            var currentReviewCount = currentReviews.Count;

            var viewModel = new MentorProfileViewModel
            {
                MentorId = mentorProfile.MentorId,
                FullName = mentorProfile.User.FullName,
                Email = mentorProfile.User.Email,
                Gender = mentorProfile.User.Gender,
                LinkedInUrl = mentorProfile.User.LinkedInUrl,
                Expertise = mentorProfile.Expertise,
                Skills = mentorProfile.Skills,
                YearsOfExperience = mentorProfile.YearsOfExperience,
                Bio = mentorProfile.Bio,
                Rating = currentRating,
                ReviewCount = currentReviewCount,
                Availability = mentorProfile.Availability,
                AvatarUrl = $"https://i.pravatar.cc/150?u={mentorProfile.MentorId}",
                MenteeCount = menteeCount,
                HasExistingRelationship = hasExistingRelationship,
                Reviews = reviewsList
            };

            return View(viewModel);
        }

        // POST: Mentor/CreateReview
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateReview([FromBody] CreateReviewViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid review data" });
            }

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Json(new { success = false, message = "Please login first" });
            }

            try
            {
                var review = new MentorReview
                {
                    MentorId = model.MentorId,
                    MenteeId = userId.Value,
                    Rating = model.Rating,
                    Comment = model.Comment,
                    CreatedAt = DateTime.Now
                };

                _context.MentorReviews.Add(review);
                await _context.SaveChangesAsync();

                // تحديث معدل التقييمات - بطريقة مختلفة
                var mentorProfile = await _context.MentorProfiles.FindAsync(model.MentorId);
                if (mentorProfile != null)
                {
                    var ratings = await _context.MentorReviews
                        .Where(r => r.MentorId == model.MentorId)
                        .Select(r => r.Rating)
                        .ToListAsync();

                    if (ratings.Any())
                    {
                        mentorProfile.Rating = (decimal)ratings.Average();
                    }

                    mentorProfile.ReviewCount = ratings.Count;
                    await _context.SaveChangesAsync();
                }

                return Json(new { success = true, message = "Review added successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error adding review: {ex.Message}" });
            }
        }

        // GET: Mentor/EditProfile
        public async Task<IActionResult> EditProfile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var mentorProfile = await _context.MentorProfiles
                .Include(m => m.User)
                .FirstOrDefaultAsync(m => m.MentorId == userId);

            var viewModel = new EditMentorProfileViewModel
            {
                FullName = mentorProfile?.User?.FullName ?? "",
                Email = mentorProfile?.User?.Email ?? "",
                Gender = mentorProfile?.User?.Gender ?? "Male",
                LinkedInUrl = mentorProfile?.User?.LinkedInUrl ?? "",
                Expertise = mentorProfile?.Expertise,
                Skills = mentorProfile?.Skills,
                YearsOfExperience = mentorProfile?.YearsOfExperience ?? 0,
                Bio = mentorProfile?.Bio,
                Availability = mentorProfile?.Availability
            };

            return View(viewModel);
        }

        // POST: Mentor/EditProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(EditMentorProfileViewModel model)
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

            var mentorProfile = await _context.MentorProfiles
                .Include(m => m.User)
                .FirstOrDefaultAsync(m => m.MentorId == userId);

            if (mentorProfile == null)
            {
                mentorProfile = new MentorProfile { MentorId = userId.Value };
                _context.MentorProfiles.Add(mentorProfile);
            }

            // Update User table
            if (mentorProfile.User != null)
            {
                mentorProfile.User.FullName = model.FullName;
                mentorProfile.User.Email = model.Email;
                mentorProfile.User.Gender = model.Gender;
                mentorProfile.User.LinkedInUrl = model.LinkedInUrl;
            }

            // Update MentorProfile table
            mentorProfile.Expertise = model.Expertise;
            mentorProfile.Skills = model.Skills;
            mentorProfile.YearsOfExperience = model.YearsOfExperience;
            mentorProfile.Bio = model.Bio;
            mentorProfile.Availability = model.Availability;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Profile updated successfully!";
            return RedirectToAction("Profile");
        }

        // GET: Mentor/Dashboard
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