using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;
using System;
using ChatApp2.Models;
using ChatApp2.Models.ChatApp2.Models;

namespace ChatApp2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsernameController : Controller
    {
        private readonly IConfiguration _configuration;

        public UsernameController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // ✅ Renders Welcome Page if PublicUsername is empty
        [HttpGet("/Username/Welcome")]
        public IActionResult Welcome()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
                return RedirectToAction("Index", "Home");

            var user = GetUserById(userId.Value);
            if (user != null && !string.IsNullOrEmpty(user.PublicUsername))
                return RedirectToAction("Index", "Dashboard");

            ViewBag.UserId = userId.Value;
            return View("~/Views/Welcome/Index.cshtml");
        }

        // ✅ Check if a username is available
        [HttpGet("IsAvailable")]
        public IActionResult IsAvailable(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return BadRequest("Username cannot be empty.");

            try
            {
                using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
                conn.Open();
                var cmd = new SqlCommand("SELECT COUNT(*) FROM T_Users WHERE PublicUsername = @username", conn);
                cmd.Parameters.AddWithValue("@username", username);
                int count = (int)cmd.ExecuteScalar();
                return Ok(new { available = count == 0 });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Server error: " + ex.Message });
            }
        }

        // ✅ Suggest usernames based on input
        [HttpGet("Suggestions")]
        public IActionResult Suggestions(string baseUsername)
        {
            var suggestions = Enumerable.Range(1, 5)
                .Select(i => $"{baseUsername}{i}")
                .ToList();

            return Ok(suggestions);
        }

        // ✅ Save selected username for the logged-in user
        [HttpPost("Save")]
        public IActionResult Save([FromBody] SaveUsernameRequest request)
        {
            var currentUserId = HttpContext.Session.GetInt32("UserId");

            if (currentUserId == null)
                return Unauthorized(new { success = false, message = "Please log in again." });

            if (string.IsNullOrWhiteSpace(request.PublicUsername))
                return BadRequest(new { success = false, message = "Username cannot be empty." });

            if (!System.Text.RegularExpressions.Regex.IsMatch(request.PublicUsername, "^[a-zA-Z0-9_]{3,15}$"))
                return BadRequest(new { success = false, message = "Username must be 3–15 characters long and contain only letters, numbers, or _." });

            try
            {
                using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
                conn.Open();

                // Check if username already exists
                var checkCmd = new SqlCommand("SELECT COUNT(*) FROM T_Users WHERE PublicUsername = @username AND Id != @id", conn);
                checkCmd.Parameters.AddWithValue("@username", request.PublicUsername);
                checkCmd.Parameters.AddWithValue("@id", currentUserId.Value);
                if ((int)checkCmd.ExecuteScalar() > 0)
                    return Conflict(new { success = false, message = "Username already taken." });

                // Save the new username
                var updateCmd = new SqlCommand("UPDATE T_Users SET PublicUsername = @username WHERE Id = @id", conn);
                updateCmd.Parameters.AddWithValue("@username", request.PublicUsername);
                updateCmd.Parameters.AddWithValue("@id", currentUserId.Value);
                int rows = updateCmd.ExecuteNonQuery();

                return Ok(new { success = rows > 0 });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Server error: " + ex.Message });
            }
        }

        // ✅ NEW: Username Search (with FriendStatus like Instagram)
        [HttpGet("SearchByUsername")]
        public IActionResult SearchByUsername(string query)
        {
            var currentUserId = HttpContext.Session.GetInt32("UserId");
            if (currentUserId == null)
                return Unauthorized(new { success = false, message = "Please log in first." });

            var users = new List<object>();

            try
            {
                using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
                conn.Open();

                using var cmd = new SqlCommand("USP_User_SearchByUsername", conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@SearchTerm", query ?? "");
                cmd.Parameters.AddWithValue("@CurrentUserId", currentUserId.Value);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    users.Add(new
                    {
                        userId = reader["UserId"],
                        publicUsername = reader["PublicUsername"],
                        fullName = reader["FullName"],
                        about = reader["AboutMe"],
                        profilePicUrl = string.IsNullOrEmpty(reader["ProfilePicUrl"].ToString())
                                         ? "/images/default-avatar.png"
                                         : reader["ProfilePicUrl"],
                        friendStatus = reader["FriendStatus"]
                    });
                }
            }
            catch (Exception ex)
            {
                // ✅ RETURN THE ERROR TO FRONTEND
                return StatusCode(500, new { success = false, message = "Server error", details = ex.Message });
            }

            return Ok(users);
        }

        // 🔧 Optional: Helper to fetch user by ID from DB
        private T_User GetUserById(int userId)
        {
            try
            {
                using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
                conn.Open();

                var cmd = new SqlCommand("SELECT * FROM T_Users WHERE Id = @id", conn);
                cmd.Parameters.AddWithValue("@id", userId);
                using var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    return new T_User
                    {
                        Id = (int)reader["Id"],
                        PublicUsername = reader["PublicUsername"].ToString(),
                        UserName = reader["UserName"].ToString(),
                        AboutMe = reader["AboutMe"].ToString(),
                        ProfilePicPath = reader["ProfilePicPath"].ToString()
                    };
                }
            }
            catch
            {
                // Log if needed
            }
            return null;
        }


        [HttpPost("SaveProfileDetails")]
        public IActionResult SaveProfileDetails([FromBody] ProfileDetailsUpdate model)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized();

            try
            {
                using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
                conn.Open();
                var cmd = new SqlCommand(@"
            UPDATE T_Users 
            SET AboutMe = @about, ProfilePicPath = @pic 
            WHERE Id = @id", conn);

                cmd.Parameters.AddWithValue("@about", model.AboutMe ?? "");
                cmd.Parameters.AddWithValue("@pic", model.ProfilePicBase64 ?? "");
                cmd.Parameters.AddWithValue("@id", userId.Value);

                cmd.ExecuteNonQuery();
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("MyProfile")]
        public IActionResult GetMyProfile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return Unauthorized();

            try
            {
                using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
                conn.Open();

                var cmd = new SqlCommand(@"
            SELECT PublicUsername, UserName, AboutMe, ProfilePicPath
            FROM T_Users
            WHERE Id = @id", conn);
                cmd.Parameters.AddWithValue("@id", userId.Value);

                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    return Ok(new
                    {
                        fullName = reader["UserName"],
                        publicUsername = reader["PublicUsername"],
                        about = reader["AboutMe"],
                        profilePicUrl = string.IsNullOrEmpty(reader["ProfilePicPath"].ToString()) ? "/images/default-avatar.png" : reader["ProfilePicPath"]
                    });
                }

                return NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }


        public class ProfileDetailsUpdate
        {
            public string AboutMe { get; set; }
            public string ProfilePicBase64 { get; set; }
            public string OnlineStatus { get; set; } // Optional if used later
        }
    }

    // DTO used for saving username
    public class SaveUsernameRequest
    {
        public string PublicUsername { get; set; }
    }
}
