using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;

[Route("api/[controller]")]
[ApiController]
public class ChatController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public ChatController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpGet("GetMessages")]
    public IActionResult GetMessages(int currentUserId, int friendUserId)
    {
        List<object> messages = new();

        using (SqlConnection con = new(_configuration.GetConnectionString("DefaultConnection")))
        {
            string query = @"
                SELECT ID, Message, MessageType, CreatedBy, CreatedOn 
                FROM T_Chat_trans 
                WHERE 
                    ((CreatedBy = @currentUserId AND ReceiverId = @friendUserId) OR
                     (CreatedBy = @friendUserId AND ReceiverId = @currentUserId))
                    AND IsActive = 1
                ORDER BY CreatedOn ASC";

            using SqlCommand cmd = new(query, con);
            cmd.Parameters.AddWithValue("@currentUserId", currentUserId);
            cmd.Parameters.AddWithValue("@friendUserId", friendUserId);

            con.Open();
            using SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                messages.Add(new
                {
                    Id = reader["ID"],
                    Text = reader["Message"],
                    Type = reader["MessageType"],
                    CreatedBy = reader["CreatedBy"],
                    Timestamp = Convert.ToDateTime(reader["CreatedOn"]).ToString("h:mm tt")
                });
            }
        }

        return Ok(messages);
    }
}
