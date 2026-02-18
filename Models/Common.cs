namespace ChatApp2.Models
{
    public class Common
    {
        public string Code { get; set; }
    }
    public class AttachmentMap
    {
        public int ID { get; set; }
        public int UserID { get; set; }
        public string MasterID { get; set; }
        public string Type { get; set; }
        public string EntityTypeID { get; set; }
        public string SectionID { get; set; }
        public string Attachment { get; set; }
        public string AttachmentName { get; set; }
        public char IsActive { get; set; }
    }
}
