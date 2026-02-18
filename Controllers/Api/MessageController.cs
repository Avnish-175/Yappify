using ChatApp2.DataAccess;
using ChatApp2.Hubs;
using ChatApp2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace ChatApp2.Controllers.Api
{
    // Sets the base route for the entire controller to "api/Message".
    // All action routes will be appended to this.
    [Route("api/[controller]")]
    [ApiController]
    public class MessageController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IHubContext<ChatHub> _hubContext;

        public MessageController(IConfiguration configuration, IHubContext<ChatHub> hubContext)
        {
            _configuration = configuration;
            _hubContext = hubContext;
        }

        /// <summary>
        /// Saves a message to the database and broadcasts via SignalR to the receiver.
        /// Endpoint: POST /api/Message/SaveMessage
        /// </summary>
        [HttpPost("SaveMessage")]
        public async Task<IActionResult> SaveMessage([FromBody] MessageRequest messageRequest)
        {
            try
            {
                // Check if the incoming data is valid
                if (messageRequest == null || string.IsNullOrWhiteSpace(messageRequest.Message))
                {
                    return BadRequest(new { success = false, error = "Invalid message data." });
                }

                // Correctly declare and initialize variables from the request object
                int senderId = messageRequest.UserId;
                int receiverId = messageRequest.ReceiverId;
                string message = messageRequest.Message;
                string type = messageRequest.MessageType ?? "text";
                string fileUrl = messageRequest.FileUrl ?? "";

                // The senderId is correctly declared here, in the scope of this method.
                // It can now be used in the call to ManageData.SaveMessage.
                ManageData obj = new ManageData();
                bool result = obj.SaveMessage(message, senderId, receiverId, type, fileUrl, senderId); // Pass senderId for both CreatedBy and UpdatedBy

                if (result)
                {
                    // ... (SignalR code remains the same) ...
                    await _hubContext.Clients.Group($"user_{receiverId}")
                        .SendAsync("ReceiveMessage", new
                        {
                            fromUserId = senderId,
                            toUserId = receiverId,
                            message,
                            type,
                            fileUrl,
                            timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                        });

                    return Ok(new { success = true });
                }
                else
                {
                    return BadRequest(new { success = false, error = "Failed to save message" });
                }
            }
            catch (Exception ex)
            {
                // This catch block will now also return a more helpful error
                return BadRequest(new { success = false, error = ex.Message });
            }
        }

        /// <summary>
        /// Search users by partial public username (used in Friend Search).
        /// Endpoint: GET /api/Message/SearchUser?keyword=...
        /// </summary>
        [HttpGet("SearchUser")]
        public IActionResult SearchUser([FromQuery] string keyword)
        {
            try
            {
                using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
                using var cmd = new SqlCommand("USP_User_SearchByUsername", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Keyword", keyword);

                conn.Open();
                using var reader = cmd.ExecuteReader();
                var users = new List<object>();

                while (reader.Read())
                {
                    users.Add(new
                    {
                        Id = reader["Id"],
                        PublicUsername = reader["PublicUsername"],
                        FullName = reader["FullName"],
                        About = reader["About"],
                        ProfilePic = reader["ProfilePic"]
                    });
                }
                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Gets the chat history between two users.
        /// Endpoint: GET /api/Message/History?user1=...&user2=...
        /// </summary>
        [HttpGet("History")]
        public IActionResult GetChatHistory([FromQuery] int user1, [FromQuery] int user2)
        {
            try
            {
                using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
                conn.Open();

                var cmd = new SqlCommand(@"
                    SELECT Message, MessageType, CreatedOn, CreatedBy
                    FROM T_Chat_trans
                    WHERE
                        (CreatedBy = @user1 AND ReceiverId = @user2)
                     OR (CreatedBy = @user2 AND ReceiverId = @user1)
                    ORDER BY CreatedOn ASC", conn);

                cmd.Parameters.AddWithValue("@user1", user1);
                cmd.Parameters.AddWithValue("@user2", user2);

                var reader = cmd.ExecuteReader();
                var messages = new List<object>();

                while (reader.Read())
                {
                    messages.Add(new
                    {
                        Message = reader["Message"].ToString(),
                        MessageType = reader["MessageType"].ToString(),
                        Time = Convert.ToDateTime(reader["CreatedOn"]),
                        SentByMe = Convert.ToInt32(reader["CreatedBy"]) == user1
                    });
                }

                return Ok(messages);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}