using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AccountService.API.Models.Audits;

[Table("Audit_Businesses")]
public class AuditBusiness : BaseAudit
{
    [Key] public int AuditId { get; set; }

    public int Id { get; set; }
    public string? Name { get; set; }
    public int? LogoBlobId { get; set; }
    public string? About { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? PhoneNumber { get; set; }
    public string? PreferredCommunicationsEmail { get; set; }
    public int? StartYear { get; set; }
    public int? PoolCount { get; set; }
}
