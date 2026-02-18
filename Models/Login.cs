namespace ChatApp2.Models
{
    public class Login
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string EmailID { get; set; }
        public string PhoneNo { get; set; }
        public string Password { get; set; } // Note: This should ideally store a HASHED password if used after hashing
        public string? PublicUsername { get; set; } // ✅ ADD THIS LINE
    }
}