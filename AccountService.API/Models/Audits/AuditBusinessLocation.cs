using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AccountService.API.Models.Audits;

[Table("Audit_BusinessLocations")]
public class AuditBusinessLocation : BaseAudit
{
    [Key] public int AuditId { get; set; }

    public int Id { get; set; }
    public int? BusinessId { get; set; }
    public int? AddressId { get; set; }
    public string? OfficeName { get; set; }
}
