using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AccountService.API.Models.Audits;

[Table("Audit_ExpertSkills")]
public class AuditExpertSkill : BaseAudit
{
    [Key] public int AuditId { get; set; }

    public int Id { get; set; }
    public string? Code { get; set; }
    public int? ExpertId { get; set; }
}
