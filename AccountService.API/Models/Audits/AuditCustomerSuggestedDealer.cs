using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AccountService.API.Models.Audits;

[Table("Audit_CustomerSuggestedDealers")]
public class AuditCustomerSuggestedDealer : BaseAudit
{
    [Key] public int AuditId { get; set; }
    public string DealerName { get; set; }
    public string DealerAddress { get; set; }
    public string DealerEmail { get; set; }
    public int? CustomerId { get; set; }
}
