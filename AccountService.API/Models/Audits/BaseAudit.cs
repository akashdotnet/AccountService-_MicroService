using System;

namespace AccountService.API.Models.Audits;

public abstract class BaseAudit
{
    public string AuditAction { get; set; }
    public DateTime AuditDate { get; set; }
    public string UserName { get; set; }
}
