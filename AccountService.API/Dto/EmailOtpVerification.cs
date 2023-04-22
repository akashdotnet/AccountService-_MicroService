namespace AccountService.API.Dto;

public class EmailOtpVerification
{
    public string? Otp { get; set; }
    public string Validity { get; set; }
    public string? Name { get; set; }
}
