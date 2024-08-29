using Microsoft.AspNetCore.Mvc;

namespace BookManagement.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
