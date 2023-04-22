using System;
using System.Collections.Generic;
using AccountService.API.Enums;
using Microsoft.EntityFrameworkCore;

namespace AccountService.API.Models;

[Index("AccountId", IsUnique = true)]
public class Customer : BaseModel
{
    public int Id { get; set; }
    public string? SanitationMethodCode { get; set; }
    public string? PoolTypeCode { get; set; }
    public string? PoolSizeCode { get; set; }
    public string? PoolMaterialCode { get; set; }
    public string? HotTubTypeCode { get; set; }
    public string? PoolSeasonCode { get; set; }
    public bool IsEmailNotificationEnabled { get; set; } = true;
    public bool IsSmsNotificationEnabled { get; set; } = true;
    public bool IsPushNotificationEnabled { get; set; } = true;
    public int AccountId { get; set; }
    public Account Account { get; set; }
    public List<CustomerAddress> CustomerAddresses { get; set; }
    public CustomerStepEnum? LastCompletedOnboardingStep { get; set; }
    public bool? ReceivePromotionalContent { get; set; }
    public bool? TermsAndConditionsAccepted { get; set; }
    public int? ProfilePhotoBlobId { get; set; }
    public bool? FirstFreeCallAvailed { get; set; }
    public DateTime? PasswordResetDate { get; set; }
    public bool IsDeleted { get; set; }
    public List<string>? HearAboutSurveys { get; set; }
}
