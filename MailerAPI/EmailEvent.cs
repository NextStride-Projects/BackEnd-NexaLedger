namespace MailerAPI;

public class EmailEvent
{
    public required string Template { get; set; }
    public required string Recipient { get; set; }
    public required string Subject { get; set; }
    public required object Data { get; set; }
}
