using ChatApp2.DataAccess;
using ChatApp2.Models;
using Microsoft.AspNetCore.Mvc;

namespace ChatApp.Controllers.Api
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        [HttpPost]
        [ActionName("SignUp")]
        public IActionResult SignUp(Login ObjLogin)
        {
            ManageData ObjManageData = new ManageData();
            bool result = ObjManageData.SignUp(ObjLogin);

            if (result)
            {
                // 🔧 Now retrieve user details to store in session (assuming you can fetch it)
                var user = ObjManageData.Login(ObjLogin.EmailID, ObjLogin.Password);
                if (user != null)
                {
                    HttpContext.Session.SetInt32("UserId", user.ID); // ✅ Store in session
                }

                return Ok(new
                {
                    success = true,
                    message = "Signup successful!"
                });
            }
            else
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Signup failed! Something went wrong."
                });
            }
        }

        //<-----For Login----->
        [HttpPost]
        [ActionName("Login")]
        public IActionResult Login(LoginRequest loginRequest)
        {
            try
            {

                ManageData ObjManageData = new ManageData();
                var user = ObjManageData.Login(loginRequest.EmailID, loginRequest.Password);

                if (user != null)
                {
                    // ✅ Store user ID in session
                    HttpContext.Session.SetInt32("UserId", user.ID);
                    return Ok(new
                    {
                        success = true,
                        message = "Login successful!",
                        data = user
                    });
                }
                else
                {
                    return Unauthorized(new
                    {
                        success = false,
                        message = "Invalid email or password!"
                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "ERROR: " + ex.Message
                });
            }
        }

        [HttpGet]
        [Route("/Login/RouteUser")]
        public IActionResult RouteUser()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Index", "Login"); // or your login page
            }

            var user = new ManageData().GetUserById(userId.Value);

            if (user != null && !string.IsNullOrEmpty(user.PublicUsername))
            {
                return RedirectToAction("Index", "Dashboard");
            }
            else
            {
                return RedirectToAction("Welcome", "Username");
            }
        }

        [HttpGet]
        [ActionName("Logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); // clear session

            // Return a redirect to login
            return Redirect("/Login/Index"); // change this based on your login page route
        }

    }
}
