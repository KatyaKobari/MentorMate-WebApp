using Microsoft.AspNetCore.Mvc;

namespace MentorMate.Controllers
{
    public class HomeController : Controller
    {
        // GET: /
        public IActionResult Index()
        {
            return View();
        }

        // GET: /Home/About
        public IActionResult About()
        {
            return View();
        }

        // GET: /Home/Contact
        public IActionResult Contact()
        {
            return View();
        }

        // GET: /Home/AccessDenied
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
