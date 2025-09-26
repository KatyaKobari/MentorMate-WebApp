using MentorMate.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System;

var builder = WebApplication.CreateBuilder(args);

//// 1️⃣ إضافة DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

// 3️⃣ إضافة Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// 4️⃣ إضافة MVC
builder.Services.AddControllersWithViews();

// 5️⃣ إضافة HttpContextAccessor
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// 6️⃣ Initialize database with sample data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        var passwordHasher = services.GetRequiredService<IPasswordHasher<User>>();

        context.Database.EnsureCreated();

        if (!context.Users.Any())
        {
            var mentorUser = new User
            {
                FullName = "Omar Mentor",
                Email = "omar@example.com",
            };
            mentorUser.PasswordHash = passwordHasher.HashPassword(mentorUser, "Password123!");

            var menteeUser = new User
            {
                FullName = "Sara Mentee",
                Email = "sara@example.com",
            };
            menteeUser.PasswordHash = passwordHasher.HashPassword(menteeUser, "Password123!");

            context.Users.AddRange(mentorUser, menteeUser);
            context.SaveChanges();

            var mentorProfile = new MentorProfile
            {
                MentorId = mentorUser.UserId,
                Expertise = "Software Development",
                Skills = "C#, ASP.NET, JavaScript, React",
                YearsOfExperience = 5
            };
            context.MentorProfiles.Add(mentorProfile);

            var menteeProfile = new MenteeProfile
            {
                MenteeId = menteeUser.UserId,
                FieldOfStudy = "Computer Engineering",
                Bio = "Passionate learner looking to grow in tech fields",
                Interests = "Web Development,UI/UX Design,AI",
                Goals = "Become a full-stack developer"
            };
            context.MenteeProfiles.Add(menteeProfile);
            context.SaveChanges();

            Console.WriteLine("Database seeded successfully with sample data!");
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

// 7️⃣ Middleware Configuration
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

// 8️⃣ Configure Routes
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// 9️⃣ Routes إضافية
app.MapControllerRoute(
    name: "menteeProfile",
    pattern: "Mentee/Profile",
    defaults: new { controller = "Mentee", action = "Profile" });

app.MapControllerRoute(
    name: "menteeEditProfile",
    pattern: "Mentee/EditProfile",
    defaults: new { controller = "Mentee", action = "EditProfile" });

app.MapControllerRoute(
    name: "menteeDashboard",
    pattern: "Mentee/Dashboard",
    defaults: new { controller = "MenteeDashBoard", action = "Index" });

app.MapControllerRoute(
    name: "mentorSpace",
    pattern: "MentorSpace",
    defaults: new { controller = "MentorSpace", action = "Index" });

app.MapControllerRoute(
    name: "mentorProfile",
    pattern: "Mentor/Profile",
    defaults: new { controller = "Mentor", action = "Profile" });

app.MapControllerRoute(
    name: "mentorEditProfile",
    pattern: "Mentor/EditProfile",
    defaults: new { controller = "Mentor", action = "EditProfile" });

app.MapControllerRoute(
    name: "mentorDashboard",
    pattern: "Mentor/Dashboard",
    defaults: new { controller = "MentorDashBoard", action = "Index" });
app.MapControllerRoute(
    name: "chat",
    pattern: "Chat/{action=Index}/{id?}",
    defaults: new { controller = "Chat" });
app.Run();