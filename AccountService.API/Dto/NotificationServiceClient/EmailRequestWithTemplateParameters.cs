namespace AccountService.API.Dto.NotificationServiceClient;

public class EmailRequestWithTemplateParameters
{
    public string RecipientAddress { get; set; }
    public string SenderAddress { get; set; }
    public string Subject { get; set; }
    public string HtmlTemplateName { get; set; }
    public dynamic HtmlTemplateParameters { get; set; }
}
