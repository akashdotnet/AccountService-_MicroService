using System;
using System.Collections.Generic;
using AccountService.API.Enums;

namespace AccountService.API.Dto.Response;

public class CustomerResponse
{
    public string FirstName { get; set; }
    public string? LastName { get; set; }
    public string Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? SanitationMethodCode { get; set; }
    public string? PoolTypeCode { get; set; }
    public string? PoolSizeCode { get; set; }
    public string? PoolMaterialCode { get; set; }
    public string? HotTubTypeCode { get; set; }
    public string? PoolSeasonCode { get; set; }
    public bool IsEmailNotificationEnabled { get; set; }
    public bool IsSmsNotificationEnabled { get; set; }
    public bool IsPushNotificationEnabled { get; set; }
    public List<CustomerAddressResponse> Addresses { get; set; } = new();
    public string ProfilePhotoUrl { get; set; }
    public bool IsOnboardingComplete { get; set; }
    public CustomerStepEnum LastCompletedOnboardingStep { get; set; }
    public bool ReceivePromotionalContent { get; set; }
    public bool TermsAndConditionsAccepted { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool? FirstFreeCallAvailed { get; set; }
    public DateTime PasswordResetDate { get; set; }
}
