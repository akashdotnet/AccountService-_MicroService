using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AccountService.API.Enums;

namespace AccountService.API.Models.Audits;

[Table("Audit_Experts")]
public class AuditExpert : BaseAudit
{
    [Key] public int AuditId { get; set; }
    public int Id { get; set; }
    public string? AgentId { get; set; }
    public string? ZipCode { get; set; }
    public int? ProfilePhotoBlobId { get; set; }
    public int? JoiningYear { get; set; }
    public string? About { get; set; }
    public int? AccountId { get; set; }
    public ExpertStepEnum? LastCompletedOnboardingStep { get; set; }
}
