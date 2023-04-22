namespace AccountService.API.Dto.NotificationServiceClient;

public class EmailRequest
{
    public string RecipientAddress { get; set; }
    public string SenderAddress { get; set; }
    public string Subject { get; set; }
    public string? PlainContent { get; set; }
    public string? HtmlContent { get; set; }
}
