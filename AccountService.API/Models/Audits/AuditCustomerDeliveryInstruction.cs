using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AccountService.API.Models.Audits;

[Table("Audit_CustomerDeliveryInstructions")]
public class AuditCustomerDeliveryInstruction : BaseAudit
{
    [Key] public int AuditId { get; set; }

    public int Id { get; set; }
    public int? CustomerId { get; set; }
    public string? SubdivisionName { get; set; }
    public string? HomeAccessDetails { get; set; }
    public string? PetInformation { get; set; }
    public string? HealthAndSafetyInformation { get; set; }
    public string? PoolOrEquipmentNotes { get; set; }
}
