using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using MentorMate.Models;
using MentorMate.ViewModels;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.Linq;
using System.Diagnostics;
using System.Text;

namespace MentorMate.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly ILogger<AccountController> _logger;

        public AccountController(AppDbContext context, IPasswordHasher<User> passwordHasher, ILogger<AccountController> logger)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _logger = logger;
        }

        // ================= Step 1 =================
        [HttpGet]
        public IActionResult RegisterStep1()
        {
            HttpContext.Session.Remove("RegisterUserId");
            HttpContext.Session.Remove("RegisterRole");

            return View(new RegisterStep1ViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterStep1(RegisterStep1ViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            if (await _context.Users.AnyAsync(u => u.Email == model.Email))
            {
                ModelState.AddModelError("Email", "Email is already taken");
                return View(model);
            }

            var user = new User
            {
                FullName = model.FullName,
                Email = model.Email,
                PasswordHash = _passwordHasher.HashPassword(null, model.Password),
                Bio = "",
                LinkedInUrl = "",
                Gender = model.Gender
            };

            try
            {
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"User saved successfully with ID: {user.UserId}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error saving user: {ex.Message}");
                ModelState.AddModelError("", "An error occurred while saving your data. Please try again.");
                return View(model);
            }

            HttpContext.Session.SetInt32("RegisterUserId", user.UserId);
            return RedirectToAction("RegisterStep2");
        }

        // ================= Step 2 =================
        [HttpGet]
        public IActionResult RegisterStep2()
        {
            if (!HttpContext.Session.GetInt32("RegisterUserId").HasValue)
                return RedirectToAction("RegisterStep1");

            return View(new RegisterStep2ViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RegisterStep2(RegisterStep2ViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            if (string.IsNullOrEmpty(model.Role))
            {
                ModelState.AddModelError("Role", "Please select a role");
                return View(model);
            }

            HttpContext.Session.SetString("RegisterRole", model.Role);
            return RedirectToAction("RegisterStep3");
        }

        // ================= Step 3 =================
        [HttpGet]
        public IActionResult RegisterStep3()
        {
            var userId = HttpContext.Session.GetInt32("RegisterUserId");
            var role = HttpContext.Session.GetString("RegisterRole");

            if (!userId.HasValue || string.IsNullOrEmpty(role))
                return RedirectToAction("RegisterStep1");

            var model = new RegisterStep3ViewModel
            {
                Role = role
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterStep3(RegisterStep3ViewModel model)
        {
            var role = HttpContext.Session.GetString("RegisterRole");
            _logger.LogInformation($"Starting RegisterStep3 for role: {role}");

            // تحقق مشروط حسب الدور
            if (role == "Mentee")
            {
                // إزالة حقول المنتور
                ModelState.Remove("Expertise");
                ModelState.Remove("Skills");
                ModelState.Remove("YearsOfExperience");
                ModelState.Remove("Availability");

                if (string.IsNullOrEmpty(model.FieldOfStudy))
                    ModelState.AddModelError("FieldOfStudy", "Field of study is required for mentees");

                if (string.IsNullOrEmpty(model.Interests))
                    ModelState.AddModelError("Interests", "Interests are required for mentees");

                if (string.IsNullOrEmpty(model.Goals))
                    ModelState.AddModelError("Goals", "Goals are required for mentees");
            }
            else if (role == "Mentor")
            {
                // إزالة حقول المنتيي
                ModelState.Remove("FieldOfStudy");
                ModelState.Remove("Interests");
                ModelState.Remove("Goals");

                if (string.IsNullOrEmpty(model.Expertise))
                    ModelState.AddModelError("Expertise", "Expertise is required for mentors");

                if (string.IsNullOrEmpty(model.Skills))
                    ModelState.AddModelError("Skills", "Skills are required for mentors");

                if (!model.YearsOfExperience.HasValue)
                    ModelState.AddModelError("YearsOfExperience", "Years of experience is required");

                if (string.IsNullOrEmpty(model.Availability))
                    ModelState.AddModelError("Availability", "Availability is required");
            }

            // التحقق من الحقول المشتركة
            if (string.IsNullOrEmpty(model.Bio))
                ModelState.AddModelError("Bio", "Bio is required");

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState is invalid");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    _logger.LogWarning($"Validation error: {error.ErrorMessage}");
                }
                return View(model);
            }

            var userId = HttpContext.Session.GetInt32("RegisterUserId");

            if (!userId.HasValue || string.IsNullOrEmpty(role))
                return RedirectToAction("RegisterStep1");

            var user = await _context.Users.FindAsync(userId.Value);
            if (user == null)
            {
                _logger.LogError($"User not found with ID: {userId.Value}");
                return RedirectToAction("RegisterStep1");
            }

            // تحديث بيانات المستخدم الأساسية
            user.Bio = string.IsNullOrWhiteSpace(model.Bio) ? "" : model.Bio;
            user.LinkedInUrl = string.IsNullOrWhiteSpace(model.LinkedInUrl) ? "" : model.LinkedInUrl;

            try
            {
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"User updated successfully with ID: {user.UserId}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating user: {ex.Message}");
                ModelState.AddModelError("", "An error occurred while updating your data. Please try again.");
                return View(model);
            }

            bool profileSaved = false;
            string errorMessage = "";

            try
            {
                if (role == "Mentor")
                {
                    var existingMentor = await _context.MentorProfiles.FindAsync(user.UserId);
                    if (existingMentor != null)
                    {
                        _context.MentorProfiles.Remove(existingMentor);
                        await _context.SaveChangesAsync();
                    }

                    var mentorProfile = new MentorProfile
                    {
                        MentorId = user.UserId,
                        Expertise = string.IsNullOrWhiteSpace(model.Expertise) ? "Not specified" : model.Expertise,
                        Skills = string.IsNullOrWhiteSpace(model.Skills) ? "Not specified" : model.Skills,
                        YearsOfExperience = model.YearsOfExperience ?? 0,
                        Availability = string.IsNullOrWhiteSpace(model.Availability) ? "Available" : model.Availability,
                        Bio = user.Bio,
                        Rating = 0,
                        ReviewCount = 0
                    };

                    _context.MentorProfiles.Add(mentorProfile);
                    await _context.SaveChangesAsync();

                    profileSaved = true;
                    _logger.LogInformation($"Mentor profile saved successfully for user ID: {user.UserId}");
                }
                else // Mentee
                {
                    var existingMentee = await _context.MenteeProfiles.FindAsync(user.UserId);
                    if (existingMentee != null)
                    {
                        _context.MenteeProfiles.Remove(existingMentee);
                        await _context.SaveChangesAsync();
                    }

                    var menteeProfile = new MenteeProfile
                    {
                        MenteeId = user.UserId,
                        Bio = string.IsNullOrWhiteSpace(model.Bio) ? "" : model.Bio,
                        FieldOfStudy = string.IsNullOrWhiteSpace(model.FieldOfStudy) ? "Not specified" : model.FieldOfStudy,
                        Interests = string.IsNullOrWhiteSpace(model.Interests) ? "Not specified" : model.Interests,
                        Goals = string.IsNullOrWhiteSpace(model.Goals) ? "Not specified" : model.Goals
                    };

                    _context.MenteeProfiles.Add(menteeProfile);
                    await _context.SaveChangesAsync();

                    profileSaved = true;
                    _logger.LogInformation($"Mentee profile saved successfully for user ID: {user.UserId}");
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                _logger.LogError($"Error saving {role} profile: {ex.Message}");
                _logger.LogError($"Stack trace: {ex.StackTrace}");

                if (ex.InnerException != null)
                {
                    _logger.LogError($"Inner exception: {ex.InnerException.Message}");
                }
            }

            if (!profileSaved)
            {
                ModelState.AddModelError("", $"Failed to save {role} profile. Error: {errorMessage}");
                return View(model);
            }

            // تسجيل الدخول تلقائي
            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetString("UserEmail", user.Email);
            HttpContext.Session.SetString("UserName", user.FullName);
            HttpContext.Session.SetString("UserRole", role);

            // تنظيف بيانات التسجيل من الجلسة
            HttpContext.Session.Remove("RegisterUserId");
            HttpContext.Session.Remove("RegisterRole");

            return RedirectToAction("Profile", role);
        }
        // ================= Login =================
        [HttpGet]
        public IActionResult Login()
        {
            // لو المستخدم أصلاً مسجل دخول، رجّعه عالبروفايل
            if (HttpContext.Session.GetInt32("UserId").HasValue)
            {
                var role = HttpContext.Session.GetString("UserRole");
                return RedirectToAction("Profile", role);
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("", "Email and password are required");
                return View();
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                ModelState.AddModelError("", "Invalid email or password");
                return View();
            }

            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
            if (result == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError("", "Invalid email or password");
                return View();
            }

            // جلب الدور (Mentor أو Mentee)
            string role = "Mentee"; // الافتراضي
            var mentor = await _context.MentorProfiles.FirstOrDefaultAsync(m => m.MentorId == user.UserId);
            if (mentor != null) role = "Mentor";

            // تخزين بيانات المستخدم بالسيشن
            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetString("UserEmail", user.Email);
            HttpContext.Session.SetString("UserName", user.FullName);
            HttpContext.Session.SetString("UserRole", role);
            HttpContext.Session.SetString("UserGender", user.Gender);

            return RedirectToAction("Profile", role);
        }


        // ================= Logout =================
        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // ================= Switch Role =================
        [HttpGet]
        public async Task<IActionResult> SwitchRole()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login");
            }

            var currentRole = HttpContext.Session.GetString("UserRole");
            var newRole = currentRole == "Mentor" ? "Mentee" : "Mentor";

            // Check if user already has a profile for the new role
            if (newRole == "Mentor")
            {
                var mentorProfile = await _context.MentorProfiles.FindAsync(userId);
                if (mentorProfile == null)
                {
                    // Create a basic mentor profile
                    mentorProfile = new MentorProfile
                    {
                        MentorId = userId.Value,
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
                }
            }
            else
            {
                var menteeProfile = await _context.MenteeProfiles.FindAsync(userId);
                if (menteeProfile == null)
                {
                    // Create a basic mentee profile
                    menteeProfile = new MenteeProfile
                    {
                        MenteeId = userId.Value,
                        FieldOfStudy = "Not specified",
                        Interests = "Not specified",
                        Goals = "Not specified"
                    };
                    _context.MenteeProfiles.Add(menteeProfile);
                    await _context.SaveChangesAsync();
                }
            }

            // Update session with new role
            HttpContext.Session.SetString("UserRole", newRole);

            // Redirect to the appropriate dashboard
            return RedirectToAction("Index", newRole == "Mentor" ? "MentorDashBoard" : "MenteeDashBoard");
        }

        
    }
}