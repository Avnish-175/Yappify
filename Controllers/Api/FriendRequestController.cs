using ChatApp2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;

namespace ChatApp2.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class FriendRequestController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public FriendRequestController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // ✅ Send a Friend Request
        [HttpPost("Send")]
        public IActionResult SendFriendRequest([FromBody] FriendRequestModel model)
        {
            var currentUserId = HttpContext.Session.GetInt32("UserId");
            if (currentUserId == null)
                return Unauthorized(new { success = false, message = "User not logged in." });

            if (model.ToUserId == 0)
                return BadRequest(new { success = false, message = "Invalid target user ID." });

            try
            {
                using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
                conn.Open();

                var checkCmd = new SqlCommand(@"
                    SELECT COUNT(*) FROM T_Friend_Request
                    WHERE 
                        (SenderId = @sender AND ReceiverId = @receiver)
                     OR (SenderId = @receiver AND ReceiverId = @sender)", conn);

                checkCmd.Parameters.AddWithValue("@sender", currentUserId.Value);
                checkCmd.Parameters.AddWithValue("@receiver", model.ToUserId);

                var existingCount = (int)checkCmd.ExecuteScalar();
                if (existingCount > 0)
                {
                    return Conflict(new { success = false, message = "Friend request already exists." });
                }

                var insertCmd = new SqlCommand(@"
                    INSERT INTO T_Friend_Request (SenderId, ReceiverId, Status, CreatedOn)
                    VALUES (@sender, @receiver, 'Pending', GETDATE())", conn);

                insertCmd.Parameters.AddWithValue("@sender", currentUserId.Value);
                insertCmd.Parameters.AddWithValue("@receiver", model.ToUserId);

                insertCmd.ExecuteNonQuery();

                return Ok(new { success = true, message = "Friend request sent." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Server error: " + ex.Message });
            }
        }

        // ✅ Accept Friend Request
        [HttpPost("Accept")]
        public IActionResult AcceptFriendRequest([FromBody] FriendRequestModel model)
        {
            var currentUserId = HttpContext.Session.GetInt32("UserId");
            if (currentUserId == null)
                return Unauthorized();

            try
            {
                using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
                conn.Open();

                var cmd = new SqlCommand(@"
                    UPDATE T_Friend_Request
                    SET Status = 'Accepted'
                    WHERE ReceiverId = @receiver AND SenderId = @sender AND Status = 'Pending'", conn);

                cmd.Parameters.AddWithValue("@receiver", currentUserId.Value);
                cmd.Parameters.AddWithValue("@sender", model.ToUserId);

                var rows = cmd.ExecuteNonQuery();
                return Ok(new { success = rows > 0 });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // ✅ Reject Friend Request
        [HttpPost("Reject")]
        public IActionResult RejectFriendRequest([FromBody] FriendRequestModel model)
        {
            var currentUserId = HttpContext.Session.GetInt32("UserId");
            if (currentUserId == null)
                return Unauthorized();

            try
            {
                using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
                conn.Open();

                var cmd = new SqlCommand(@"
                    DELETE FROM T_Friend_Request
                    WHERE ReceiverId = @receiver AND SenderId = @sender AND Status = 'Pending'", conn);

                cmd.Parameters.AddWithValue("@receiver", currentUserId.Value);
                cmd.Parameters.AddWithValue("@sender", model.ToUserId);

                var rows = cmd.ExecuteNonQuery();
                return Ok(new { success = rows > 0 });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPost("Respond")]
        public IActionResult RespondToRequest([FromBody] FriendRequestResponseModel model)
        {
            using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            conn.Open();

            var cmd = new SqlCommand(@"
                UPDATE T_Friend_Request
                SET Status = @status
                WHERE Id = @requestId", conn);

            cmd.Parameters.AddWithValue("@status", model.Status); // 'Accepted' or 'Rejected'
            cmd.Parameters.AddWithValue("@requestId", model.RequestId);

            cmd.ExecuteNonQuery();
            return Ok(new { success = true });
        }

        // ✅ Get friend requests received by the current user
        [HttpGet("Received")]
        public IActionResult GetReceivedRequests()
        {
            var currentUserId = HttpContext.Session.GetInt32("UserId");
            if (currentUserId == null)
                return Unauthorized(new { success = false, message = "User not logged in." });

            var requests = new List<FriendRequestViewModel>();

            using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            conn.Open();

            var cmd = new SqlCommand(@"
                SELECT 
                    F.Id AS RequestId,
                    U.Id AS SenderId,
                    U.UserName,
                    U.PublicUsername,
                    U.AboutMe,
                    U.ProfilePicPath
                FROM T_Friend_Request F
                INNER JOIN T_Users U ON U.Id = F.SenderId
                WHERE F.ReceiverId = @receiverId AND F.Status = 'Pending'
            ", conn);

            cmd.Parameters.AddWithValue("@receiverId", currentUserId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                requests.Add(new FriendRequestViewModel
                {
                    RequestId = reader.GetInt32(0),
                    SenderId = reader.GetInt32(1),
                    UserName = reader.GetString(2),
                    PublicUsername = reader.GetString(3),
                    AboutMe = reader.IsDBNull(4) ? "" : reader.GetString(4),
                    ProfilePicUrl = reader.IsDBNull(5) ? null : reader.GetString(5)
                });
            }

            return Ok(requests);
        }

        [HttpGet("Friends")]
        public IActionResult GetFriends()
        {
            var currentUserId = HttpContext.Session.GetInt32("UserId");
            if (currentUserId == null)
                return Unauthorized(new { success = false, message = "User not logged in." });

            var friends = new List<FriendRequestViewModel>();

            using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            conn.Open();

            var cmd = new SqlCommand(@"
        SELECT 
            U.Id AS FriendId,
            U.PublicUsername,
            U.AboutMe,
            U.ProfilePicPath
        FROM T_Friend_Request F
        INNER JOIN T_Users U 
            ON U.Id = CASE 
                        WHEN F.SenderId = @userId THEN F.ReceiverId 
                        ELSE F.SenderId 
                     END
        WHERE 
            (F.SenderId = @userId OR F.ReceiverId = @userId)
            AND F.Status = 'Accepted'
    ", conn);

            cmd.Parameters.AddWithValue("@userId", currentUserId.Value);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                friends.Add(new FriendRequestViewModel
                {
                    // SenderId = reader.GetInt32(0), ❌ Don't use this for friends
                    UserId = reader.GetInt32(0),    // ✅ Set this instead
                    PublicUsername = reader.GetString(1),
                    AboutMe = reader.IsDBNull(2) ? "" : reader.GetString(2),
                    ProfilePicUrl = reader.IsDBNull(3) ? null : reader.GetString(3)
                });

            }

            return Ok(friends);
        }


        // ✅ Model for incoming request
        public class FriendRequestModel
        {
            public int ToUserId { get; set; }
        }

        public class FriendRequestResponseModel
        {
            public int RequestId { get; set; }
            public string Status { get; set; } // 'Accepted' or 'Rejected'
        }
    }
}
