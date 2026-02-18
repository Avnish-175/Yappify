namespace ChatApp2.Models
{
    public class SaveUsernameRequest
    {
        // ✅ REMOVE UserId here. It will be obtained from the authenticated user on the server.
        public required string PublicUsername { get; set; } // Renamed from 'Username' for consistency
    }
}