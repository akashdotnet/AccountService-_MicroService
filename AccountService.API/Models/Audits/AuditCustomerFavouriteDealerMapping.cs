using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AccountService.API.Models.Audits;

[Table("Audit_CustomerFavouriteDealerMappings")]
public class AuditCustomerFavouriteDealerMapping : BaseAudit
{
    [Key] public int AuditId { get; set; }

    public int Id { get; set; }
    public int? BusinessLocationId { get; set; }
    public int? CustomerId { get; set; }
}
