using Microsoft.AspNetCore.Mvc;

namespace ChatApp2.Controllers
{
    public class LoginController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
