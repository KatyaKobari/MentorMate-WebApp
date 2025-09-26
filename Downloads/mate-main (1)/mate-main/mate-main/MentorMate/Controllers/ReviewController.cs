using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using MentorMate.Models;
using System;

namespace MentorMate.Controllers
{
    public class ReviewController : Controller
    {
        private readonly AppDbContext _context;

        public ReviewController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> AddReview([FromBody] AddReviewRequest request)
        {
            if (!HttpContext.Session.GetInt32("UserId").HasValue)
                return Json(new { success = false, message = "Not authenticated" });

            var menteeId = HttpContext.Session.GetInt32("UserId").Value;

            // Check if mentee has an approved relationship with the mentor
            var hasApprovedRelationship = await _context.MentorshipRequests
                .AnyAsync(r => r.Status == "Approved" && 
                              r.MentorId == request.MentorId && 
                              r.MenteeId == menteeId);

            if (!hasApprovedRelationship)
                return Json(new { success = false, message = "You can only review mentors you have an approved relationship with" });

            // Check if mentee has already reviewed this mentor
            var existingReview = await _context.MentorReviews
                .FirstOrDefaultAsync(r => r.MentorId == request.MentorId && r.MenteeId == menteeId);

            if (existingReview != null)
                return Json(new { success = false, message = "You have already reviewed this mentor" });

            try
            {
                var review = new MentorReview
                {
                    MentorId = request.MentorId,
                    MenteeId = menteeId,
                    Rating = request.Rating,
                    Comment = request.Comment?.Trim(),
                    CreatedAt = DateTime.Now
                };

                _context.MentorReviews.Add(review);
                await _context.SaveChangesAsync();

                // Update mentor's average rating and review count
                var mentor = await _context.MentorProfiles.FirstOrDefaultAsync(m => m.MentorId == request.MentorId);
                if (mentor != null)
                {
                    var allReviews = await _context.MentorReviews.Where(r => r.MentorId == request.MentorId).ToListAsync();
                    mentor.ReviewCount = allReviews.Count;
                    mentor.Rating = allReviews.Any() ? (decimal)allReviews.Average(r => r.Rating) : 0;
                    await _context.SaveChangesAsync();
                }

                return Json(new { success = true, message = "Review added successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred while adding the review" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetMentorReviews(int mentorId)
        {
            try
            {
                var reviews = await _context.MentorReviews
                    .Include(r => r.Mentee)
                    .Where(r => r.MentorId == mentorId)
                    .OrderByDescending(r => r.CreatedAt)
                    .Select(r => new
                    {
                        r.ReviewId,
                        r.Rating,
                        r.Comment,
                        r.CreatedAt,
                        MenteeName = r.Mentee.FullName
                    })
                    .ToListAsync();

                var averageRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0;
                var totalReviews = reviews.Count;

                return Json(new { 
                    success = true, 
                    data = new { 
                        reviews, 
                        averageRating = Math.Round(averageRating, 1), 
                        totalReviews 
                    } 
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred while loading reviews" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateReview([FromBody] UpdateReviewRequest request)
        {
            if (!HttpContext.Session.GetInt32("UserId").HasValue)
                return Json(new { success = false, message = "Not authenticated" });

            var menteeId = HttpContext.Session.GetInt32("UserId").Value;

            try
            {
                var review = await _context.MentorReviews
                    .FirstOrDefaultAsync(r => r.ReviewId == request.ReviewId && r.MenteeId == menteeId);

                if (review == null)
                    return Json(new { success = false, message = "Review not found" });

                review.Rating = request.Rating;
                review.Comment = request.Comment?.Trim();
                review.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Review updated successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred while updating the review" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteReview(int reviewId)
        {
            if (!HttpContext.Session.GetInt32("UserId").HasValue)
                return Json(new { success = false, message = "Not authenticated" });

            var menteeId = HttpContext.Session.GetInt32("UserId").Value;

            try
            {
                var review = await _context.MentorReviews
                    .FirstOrDefaultAsync(r => r.ReviewId == reviewId && r.MenteeId == menteeId);

                if (review == null)
                    return Json(new { success = false, message = "Review not found" });

                _context.MentorReviews.Remove(review);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Review deleted successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred while deleting the review" });
            }
        }
    }

    public class AddReviewRequest
    {
        public int MentorId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
    }

    public class UpdateReviewRequest
    {
        public int ReviewId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
    }
}
