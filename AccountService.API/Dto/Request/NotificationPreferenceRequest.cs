namespace AccountService.API.Dto.Request;

public class NotificationPreferenceRequest
{
    public bool IsEmailNotificationEnabled { get; set; }
    public bool IsSmsNotificationEnabled { get; set; }
    public bool IsPushNotificationEnabled { get; set; }
}
