namespace AccountService.API.Dto.Request;

public class ValidateOtpRequest : Session
{
    public string Otp { get; set; }
}
