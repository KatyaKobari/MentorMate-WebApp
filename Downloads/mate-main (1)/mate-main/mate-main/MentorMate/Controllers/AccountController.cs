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

            // جلب الدور (Mentor أو Mentee) - الأولوية للمنتور إذا كان لديه ملف شخصي
            string role = "Mentee"; // الافتراضي
            var mentor = await _context.MentorProfiles.FirstOrDefaultAsync(m => m.MentorId == user.UserId);
            var mentee = await _context.MenteeProfiles.FirstOrDefaultAsync(m => m.MenteeId == user.UserId);
            
            if (mentor != null) 
            {
                role = "Mentor";
            }
            else if (mentee != null)
            {
                role = "Mentee";
            }
            // إذا لم يكن لديه أي ملف شخصي، يبقى Mentee كافتراضي

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
        [Route("Account/SwitchRole")]
        public async Task<IActionResult> SwitchRole()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                _logger.LogWarning("SwitchRole called without valid user session");
                return RedirectToAction("Login");
            }

            var currentRole = HttpContext.Session.GetString("UserRole");
            if (string.IsNullOrEmpty(currentRole))
            {
                _logger.LogWarning($"User {userId} has no role in session");
                return RedirectToAction("Login");
            }

            var newRole = currentRole == "Mentor" ? "Mentee" : "Mentor";
            
            _logger.LogInformation($"SwitchRole called - UserId: {userId}, CurrentRole: {currentRole}, NewRole: {newRole}");

            try
            {
                _logger.LogInformation($"User {userId} attempting to switch from {currentRole} to {newRole}");

                // Check if user exists in database
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    _logger.LogError($"User {userId} not found in database");
                    TempData["ErrorMessage"] = "User not found. Please log in again.";
                    return RedirectToAction("Login");
                }
                _logger.LogInformation($"User {userId} found: {user.FullName} ({user.Email})");

                // Check if user already has both profiles
                var hasMentorProfile = await _context.MentorProfiles.AnyAsync(m => m.MentorId == userId);
                var hasMenteeProfile = await _context.MenteeProfiles.AnyAsync(m => m.MenteeId == userId);
                
                _logger.LogInformation($"User {userId} - Has Mentor Profile: {hasMentorProfile}, Has Mentee Profile: {hasMenteeProfile}");

                // If user already has the profile for the new role, just switch the session
                if ((newRole == "Mentor" && hasMentorProfile) || (newRole == "Mentee" && hasMenteeProfile))
                {
                    _logger.LogInformation($"User {userId} already has {newRole} profile, switching session only");
                    HttpContext.Session.SetString("UserRole", newRole);
                    TempData["SuccessMessage"] = $"Successfully switched to {newRole} role!";
                    return RedirectToAction("Index", newRole == "Mentor" ? "MentorDashBoard" : "MenteeDashBoard");
                }

                // Create profile for the new role if it doesn't exist
                if (newRole == "Mentor")
                {
                    _logger.LogInformation($"Checking if user {userId} needs a mentor profile...");
                    var mentorProfile = await _context.MentorProfiles.FindAsync(userId);
                    if (mentorProfile == null)
                    {
                        _logger.LogInformation($"User {userId} does NOT have mentor profile. Creating new one...");
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
                        _logger.LogInformation($"Added mentor profile to context for user ID: {userId}");
                    }
                    else
                    {
                        _logger.LogInformation($"User {userId} already has mentor profile");
                    }
                }
                else // newRole == "Mentee"
                {
                    _logger.LogInformation($"Checking if user {userId} needs a mentee profile...");
                    var menteeProfile = await _context.MenteeProfiles.FindAsync(userId);
                    if (menteeProfile == null)
                    {
                        _logger.LogInformation($"User {userId} does NOT have mentee profile. Creating new one...");
                        // Create a basic mentee profile
                        menteeProfile = new MenteeProfile
                        {
                            MenteeId = userId.Value,
                            Bio = "Not specified",
                            FieldOfStudy = "Not specified",
                            Interests = "Not specified",
                            Goals = "Not specified"
                        };
                        _context.MenteeProfiles.Add(menteeProfile);
                        _logger.LogInformation($"Added mentee profile to context for user ID: {userId}");
                    }
                    else
                    {
                        _logger.LogInformation($"User {userId} already has mentee profile");
                    }
                }

                // Save all changes at once
                _logger.LogInformation($"Saving changes to database for user {userId}...");
                try
                {
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Successfully saved profile changes for user {userId}");
                }
                catch (Exception saveEx)
                {
                    _logger.LogError($"Database save error for user {userId}: {saveEx.Message}");
                    _logger.LogError($"Inner exception: {saveEx.InnerException?.Message}");
                    _logger.LogError($"Inner exception details: {saveEx.InnerException?.ToString()}");
                    _logger.LogError($"Stack trace: {saveEx.StackTrace}");
                    
                    // Try to get more specific error information
                    var errorMessage = saveEx.Message;
                    if (saveEx.InnerException != null)
                    {
                        errorMessage += $" Inner: {saveEx.InnerException.Message}";
                    }
                    
                    TempData["ErrorMessage"] = $"Database error while creating {newRole} profile: {errorMessage}";
                    return RedirectToAction("Index", currentRole == "Mentor" ? "MentorDashBoard" : "MenteeDashBoard");
                }

                // Verify the profile was created
                _logger.LogInformation($"Verifying {newRole} profile was created for user {userId}...");
                var profileExists = newRole == "Mentor" 
                    ? await _context.MentorProfiles.AnyAsync(m => m.MentorId == userId)
                    : await _context.MenteeProfiles.AnyAsync(m => m.MenteeId == userId);
                
                if (!profileExists)
                {
                    _logger.LogError($"CRITICAL: Failed to create {newRole} profile for user {userId}");
                    TempData["ErrorMessage"] = $"Failed to create {newRole} profile. Please try again.";
                    return RedirectToAction("Index", currentRole == "Mentor" ? "MentorDashBoard" : "MenteeDashBoard");
                }
                
                _logger.LogInformation($"SUCCESS: Verified {newRole} profile exists for user {userId}");

                // Update session with new role
                HttpContext.Session.SetString("UserRole", newRole);
                _logger.LogInformation($"User {userId} successfully switched from {currentRole} to {newRole}");

                // Force session to be saved
                await HttpContext.Session.CommitAsync();
                _logger.LogInformation($"Session committed for user {userId}");

                // Verify the session was updated
                var updatedRole = HttpContext.Session.GetString("UserRole");
                if (updatedRole != newRole)
                {
                    _logger.LogError($"Session update failed for user {userId}. Expected: {newRole}, Actual: {updatedRole}");
                    TempData["ErrorMessage"] = "Failed to update session. Please try again.";
                    return RedirectToAction("Index", currentRole == "Mentor" ? "MentorDashBoard" : "MenteeDashBoard");
                }
                
                _logger.LogInformation($"Session verification successful for user {userId}. Role: {updatedRole}");

                // Set success message
                if ((newRole == "Mentor" && !hasMentorProfile) || (newRole == "Mentee" && !hasMenteeProfile))
                {
                    TempData["SuccessMessage"] = $"Successfully switched to {newRole} role! A new {newRole.ToLower()} profile has been created for you.";
                }
                else
                {
                    TempData["SuccessMessage"] = $"Successfully switched to {newRole} role!";
                }

                // Redirect to the appropriate dashboard
                var redirectController = newRole == "Mentor" ? "MentorDashBoard" : "MenteeDashBoard";
                _logger.LogInformation($"FINAL REDIRECT: User {userId} switching from {currentRole} to {newRole}, redirecting to {redirectController}");
                
                // Double-check the session before redirect
                var finalSessionRole = HttpContext.Session.GetString("UserRole");
                _logger.LogInformation($"FINAL SESSION CHECK: Session role is {finalSessionRole}, expected {newRole}");
                
                return RedirectToAction("Index", redirectController);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error switching role for user {userId}: {ex.Message}");
                _logger.LogError($"Stack trace: {ex.StackTrace}");
                
                if (ex.InnerException != null)
                {
                    _logger.LogError($"Inner exception: {ex.InnerException.Message}");
                    _logger.LogError($"Inner exception stack trace: {ex.InnerException.StackTrace}");
                }
                
                // Check if it's a database constraint error
                string errorMessage = "An error occurred while switching roles.";
                if (ex.Message.Contains("constraint") || ex.Message.Contains("duplicate"))
                {
                    errorMessage = "Unable to switch roles. Please contact support if this issue persists.";
                }
                else if (ex.InnerException != null)
                {
                    errorMessage = $"Database error: {ex.InnerException.Message}";
                }
                else
                {
                    errorMessage = $"Error: {ex.Message}";
                }
                
                // If there's an error, redirect back to current dashboard
                TempData["ErrorMessage"] = errorMessage;
                
                // Try to determine the current dashboard based on existing profiles
                var hasMentorProfile = await _context.MentorProfiles.AnyAsync(m => m.MentorId == userId);
                var hasMenteeProfile = await _context.MenteeProfiles.AnyAsync(m => m.MenteeId == userId);
                
                if (hasMentorProfile && !hasMenteeProfile)
                {
                    return RedirectToAction("Index", "MentorDashBoard");
                }
                else if (hasMenteeProfile && !hasMentorProfile)
                {
                    return RedirectToAction("Index", "MenteeDashBoard");
                }
                else
                {
                    // Default fallback
                    return RedirectToAction("Index", currentRole == "Mentor" ? "MentorDashBoard" : "MenteeDashBoard");
                }
            }
        }

        // ================= Test Switch Role =================
        [HttpGet]
        public IActionResult TestSwitch()
        {
            return View();
        }

        // ================= Debug Session =================
        [HttpGet]
        public IActionResult DebugSession()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");
            var userName = HttpContext.Session.GetString("UserName");
            var userEmail = HttpContext.Session.GetString("UserEmail");
            var userGender = HttpContext.Session.GetString("UserGender");
            
            var debugInfo = new
            {
                UserId = userId,
                UserRole = userRole,
                UserName = userName,
                UserEmail = userEmail,
                UserGender = userGender,
                SessionId = HttpContext.Session.Id,
                IsAvailable = HttpContext.Session.IsAvailable
            };
            
            return Json(debugInfo);
        }

        // ================= Test New Mentor to Mentee =================
        [HttpGet]
        public async Task<IActionResult> TestNewMentorToMentee()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Json(new { success = false, message = "User not logged in" });
            }

            try
            {
                _logger.LogInformation($"Testing new mentor to mentee conversion for user {userId}");
                
                // Check current profiles
                var hasMentorProfile = await _context.MentorProfiles.AnyAsync(m => m.MentorId == userId);
                var hasMenteeProfile = await _context.MenteeProfiles.AnyAsync(m => m.MenteeId == userId);
                
                _logger.LogInformation($"User {userId} - Has Mentor: {hasMentorProfile}, Has Mentee: {hasMenteeProfile}");
                
                if (!hasMenteeProfile)
                {
                    _logger.LogInformation($"Creating mentee profile for user {userId}");
                    var menteeProfile = new MenteeProfile
                    {
                        MenteeId = userId.Value,
                        Bio = "Not specified",
                        FieldOfStudy = "Not specified",
                        Interests = "Not specified",
                        Goals = "Not specified"
                    };
                    
                    _context.MenteeProfiles.Add(menteeProfile);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Successfully created mentee profile for user {userId}");
                }
                
                // Update session to Mentee
                HttpContext.Session.SetString("UserRole", "Mentee");
                await HttpContext.Session.CommitAsync();
                
                return Json(new { 
                    success = true, 
                    message = "Successfully created mentee profile and switched role",
                    hasMentorProfile = hasMentorProfile,
                    hasMenteeProfile = true,
                    newRole = "Mentee"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in TestNewMentorToMentee for user {userId}: {ex.Message}");
                _logger.LogError($"Stack trace: {ex.StackTrace}");
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        // ================= Force Create Profile =================
        [HttpGet]
        public async Task<IActionResult> ForceCreateProfile(string role)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Json(new { success = false, message = "User not logged in" });
            }

            try
            {
                if (role == "Mentor")
                {
                    var existingProfile = await _context.MentorProfiles.FindAsync(userId);
                    if (existingProfile == null)
                    {
                        var mentorProfile = new MentorProfile
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
                        return Json(new { success = true, message = "Mentor profile created successfully" });
                    }
                    else
                    {
                        return Json(new { success = true, message = "Mentor profile already exists" });
                    }
                }
                else if (role == "Mentee")
                {
                    var existingProfile = await _context.MenteeProfiles.FindAsync(userId);
                    if (existingProfile == null)
                    {
                        var menteeProfile = new MenteeProfile
                        {
                            MenteeId = userId.Value,
                            FieldOfStudy = "Not specified",
                            Interests = "Not specified",
                            Goals = "Not specified"
                        };
                        _context.MenteeProfiles.Add(menteeProfile);
                        await _context.SaveChangesAsync();
                        return Json(new { success = true, message = "Mentee profile created successfully" });
                    }
                    else
                    {
                        return Json(new { success = true, message = "Mentee profile already exists" });
                    }
                }
                else
                {
                    return Json(new { success = false, message = "Invalid role specified" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating {role} profile: {ex.Message}");
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        
    }
}