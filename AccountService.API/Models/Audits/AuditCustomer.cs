using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AccountService.API.Enums;

namespace AccountService.API.Models.Audits;

[Table("Audit_Customers")]
public class AuditCustomer : BaseAudit
{
    [Key] public int AuditId { get; set; }

    public int Id { get; set; }
    public string? SanitationMethodCode { get; set; }
    public string? PoolTypeCode { get; set; }
    public string? PoolSizeCode { get; set; }
    public string? PoolMaterialCode { get; set; }
    public string? HotTubTypeCode { get; set; }
    public string? PoolSeasonCode { get; set; }
    public bool? IsEmailNotificationEnabled { get; set; }
    public bool? IsSmsNotificationEnabled { get; set; }
    public bool? IsPushNotificationEnabled { get; set; }
    public int? AccountId { get; set; }
    public bool? ReceivePromotionalContent { get; set; }
    public bool? TermsAndConditionsAccepted { get; set; }
    public CustomerStepEnum? LastCompletedOnboardingStep { get; set; }
    public int? ProfilePhotoBlobId { get; set; }
    public bool? FirstFreeCallAvailed { get; set; }
    public DateTime? PasswordResetDate { get; set; }
    public bool? IsDeleted { get; set; }
}
