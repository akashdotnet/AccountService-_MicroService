using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AccountService.API.Models.Audits;

[Table("Audit_Addresses")]
public class AuditAddress : BaseAudit
{
    [Key] public int AuditId { get; set; }

    public int Id { get; set; }
    public string? AddressValue { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
    public bool? IsPrimaryAddress { get; set; }
}
