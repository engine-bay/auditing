namespace EngineBay.Auditing;

using EngineBay.Persistence;
using System;

public class AuditEntryDto
{
    public AuditEntryDto(AuditEntry auditEntry)
    {
        if (auditEntry is null)
        {
            throw new ArgumentNullException(nameof(auditEntry));
        }

        this.Id = auditEntry.Id;
        this.ApplicationUserId = auditEntry.ApplicationUserId ?? Guid.Empty;
        this.ApplicationUserName = auditEntry.ApplicationUserName ?? string.Empty;
        this.EntityName = auditEntry.EntityName ?? string.Empty;
        this.ActionType = auditEntry.ActionType ?? string.Empty;
        this.EntityId = auditEntry.EntityId ?? string.Empty;
        this.Changes = auditEntry.Changes ?? string.Empty;
        this.CreatedAt = auditEntry.CreatedAt;
    }

    public Guid Id { get; set; }

    public Guid ApplicationUserId { get; set; }

    public string? ApplicationUserName { get; set; }

    public string ActionType { get; set; }

    public string EntityName { get; set; }

    public string EntityId { get; set; }

    public string Changes { get; set; }

    public DateTime CreatedAt { get; set; }
}
