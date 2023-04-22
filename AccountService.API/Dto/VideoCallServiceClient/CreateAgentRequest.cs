namespace AccountService.API.Dto.VideoCallServiceClient;

public class CreateAgentRequest
{
    public string Email { get; set; }
    public string Name { get; set; }
    public string? PhoneNumber { get; set; }
}
