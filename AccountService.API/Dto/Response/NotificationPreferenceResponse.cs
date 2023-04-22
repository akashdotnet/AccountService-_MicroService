namespace AccountService.API.Dto.Response;

public class NotificationPreferenceResponse
{
    public bool IsEmailNotificationEnabled { get; set; }
    public bool IsSmsNotificationEnabled { get; set; }
    public bool IsPushNotificationEnabled { get; set; }
}
