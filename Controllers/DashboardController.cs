using Microsoft.AspNetCore.Mvc;

namespace ChatApp2.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
