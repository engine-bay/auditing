using EngineBay.Persistence;

namespace EngineBay.Auditing
{
    public class CreateAuditEntryRequest
    {
        public const int FullNameMaxLength = 256;

        public const int EntityNameMaxLength = 256;

        public const int ActionTypeMaxLength = 64;

        public const int EntityIdMaxLength = 64;

        public CreateAuditEntryRequest()
        {
            this.ApplicationUserName = string.Empty;
            this.EntityName = string.Empty;
            this.ActionType = string.Empty;
            this.EntityId = string.Empty;
            this.Changes = string.Empty;
        }

        public Guid ApplicationUserId { get; set; }

        public string ApplicationUserName { get; set; }

        public string EntityName { get; set; }

        public string ActionType { get; set; }

        public string EntityId { get; set; }

        public string Changes { get; set; }

        public AuditEntry ToDomainModel()
        {
            var user = new ApplicationUser();
            user.Username = this.ApplicationUserName;

            var auditEntry = new AuditEntry
            {
                ApplicationUserId = this.ApplicationUserId,
                ApplicationUser = user,
                EntityName = this.EntityName,
                ActionType = this.ActionType,
                EntityId = this.EntityId,
                Changes = this.Changes,
            };

            return auditEntry;
        }
    }
}