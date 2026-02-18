namespace ChatApp2.Models
{
    public class FriendRequestViewModel
    {
        public int RequestId { get; set; }        // for friend requests
        public int SenderId { get; set; }         // for friend requests
        public int UserId { get; set; }           // ✅ needed for friends list
        public string UserName { get; set; }
        public string PublicUsername { get; set; }
        public string AboutMe { get; set; }
        public string ProfilePicUrl { get; set; }
    }
}
