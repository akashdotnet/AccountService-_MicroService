using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PodCommonsLibrary.Core.Enums;

namespace AccountService.API.Models.Audits;

[Table("Audit_Accounts")]
public class AuditAccount : BaseAudit
{
    [Key] public int AuditId { get; set; }

    public int Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public bool? IsOnboardingComplete { get; set; }
    public UserRoleEnum UserRole { get; set; }
}
