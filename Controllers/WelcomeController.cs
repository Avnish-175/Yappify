using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;

namespace ChatApp2.Controllers
{
    public class WelcomeController : Controller
    {
        private readonly IConfiguration _configuration;

        public WelcomeController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Login");

            string? publicUsername = null;

            using (var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            using (var cmd = new SqlCommand("SELECT PublicUsername FROM T_Users WHERE ID = @id", conn))
            {
                cmd.Parameters.AddWithValue("@id", userId);
                conn.Open();
                var result = cmd.ExecuteScalar();
                if (result != DBNull.Value && result != null)
                    publicUsername = result.ToString();
            }

            ViewBag.PublicUsername = publicUsername;
            ViewBag.UserId = userId;

            return View(); // ✅ Always show Welcome first
        }

    }
}
