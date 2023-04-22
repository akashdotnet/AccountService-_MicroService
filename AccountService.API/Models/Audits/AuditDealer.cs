using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AccountService.API.Enums;

namespace AccountService.API.Models.Audits;

[Table("Audit_Dealers")]
public class AuditDealer : BaseAudit
{
    [Key] public int AuditId { get; set; }
    public int Id { get; set; }
    public int? AccountId { get; set; }
    public DealerStepEnum? LastCompletedOnboardingStep { get; set; }
    public int? BusinessId { get; set; }
    public bool? TermsAndConditionsAccepted { get; set; }
    public bool? ReceivePromotionalContent { get; set; }
}
