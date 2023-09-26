
namespace EngineBay.Auditing
{

    using EngineBay.Core;
    using FluentValidation;
    using System.Security.Claims;

    public class CreateAuditEntry : ICommandHandler<CreateAuditEntryRequest, AuditEntryDto>
    {

        private readonly AuditingWriteDbContext dbContext;
        private readonly IValidator<CreateAuditEntryRequest> validator;

        public CreateAuditEntry(AuditingWriteDbContext dbContext, IValidator<CreateAuditEntryRequest> validator)
        {
            this.dbContext = dbContext;
            this.validator = validator;
        }

        public async Task<AuditEntryDto> Handle(CreateAuditEntryRequest createAuditEntryRequest, ClaimsPrincipal user, CancellationToken cancellation)
        {
            if (createAuditEntryRequest == null)
            {
                throw new ArgumentNullException(nameof(createAuditEntryRequest));
            }

            this.validator.ValidateAndThrow(createAuditEntryRequest);
            var auditEntry = createAuditEntryRequest.ToDomainModel();

            await this.dbContext.AuditEntries.AddAsync(auditEntry, cancellation);
            await this.dbContext.SaveChangesAsync(cancellation);
            return new AuditEntryDto(auditEntry);
        }
    }
}