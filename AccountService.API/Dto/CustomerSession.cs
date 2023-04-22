using System;
using AccountService.API.Dto.Request;
using AccountService.API.Enums;

namespace AccountService.API.Dto;

public class CustomerSession
{
    public bool EmailVerified { get; set; }
    public DateTime? OtpGenerationDateTime { get; set; }
    public string? Otp { get; set; }
    public string Email { get; set; }

    public bool OtpVerified { get; set; }
    public CustomerRegistrationRequest? CustomerRegistrationRequest { get; set; }

    public EmailTemplateEnum Type { get; set; }
}
