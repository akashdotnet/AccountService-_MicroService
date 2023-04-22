using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AccountService.API.Models.Audits;

[Table("Audit_CustomerAddresses")]
public class AuditCustomerAddress : BaseAudit
{
    [Key] public int AuditId { get; set; }

    public int Id { get; set; }
    public int? CustomerId { get; set; }
    public int? AddressId { get; set; }
}
