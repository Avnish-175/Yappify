using System.Data;
using System.Data.SqlClient;

public class UserRepository
{
    private readonly string _connectionString;

    public UserRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    public void AddUser(string username, string phone, string email, string password, string createdBy)
    {
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            SqlCommand cmd = new SqlCommand("USP_User_SignUp", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@UserName", username);
            cmd.Parameters.AddWithValue("@UserPhoneNo", phone);
            cmd.Parameters.AddWithValue("@UserEmail", email);
            cmd.Parameters.AddWithValue("@UserPassword", password);
            cmd.Parameters.AddWithValue("@CreatedBy", createdBy);

            conn.Open();
            cmd.ExecuteNonQuery();
        }
    }
}
