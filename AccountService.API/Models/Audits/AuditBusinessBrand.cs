using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AccountService.API.Models.Audits;

[Table("Audit_BusinessBrands")]
public class AuditBusinessBrand : BaseAudit
{
    [Key] public int AuditId { get; set; }

    public int Id { get; set; }
    public int? BusinessId { get; set; }
    public string? Code { get; set; }
}
