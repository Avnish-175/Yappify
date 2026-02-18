public class MessageRequest
{
    public int UserId { get; set; }          // Sender
    public int ReceiverId { get; set; }      // Receiver
    public string Message { get; set; }      // Actual Message
    public string MessageType { get; set; }  // "text" or "image"
    public string FileUrl { get; set; }      // Only if image/file
}
