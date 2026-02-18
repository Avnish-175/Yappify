namespace ChatApp2.Models
{
    public class Report
    {
        public string ReportType { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string Location { get; set; }
        public string UserID { get; set; }
        public string StatusID { get; set; }
    }
    public class PayLoad
    {
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string Key { get; set; }
    }
}
