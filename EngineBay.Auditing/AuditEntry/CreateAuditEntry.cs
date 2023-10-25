namespace EngineBay.Auditing
{
    using EngineBay.Core;
    using FluentValidation;

    public class CreateAuditEntry : IClaimlessCommandHandler<CreateAuditEntryRequest, AuditEntryDto>
    {
        private readonly AuditingWriteDbContext dbContext;
        private readonly IValidator<CreateAuditEntryRequest> validator;

        public CreateAuditEntry(AuditingWriteDbContext dbContext, IValidator<CreateAuditEntryRequest> validator)
        {
            this.dbContext = dbContext;
            this.validator = validator;
        }

        public async Task<AuditEntryDto> Handle(CreateAuditEntryRequest command, CancellationToken cancellation)
        {
            if (command is null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            await this.validator.ValidateAndThrowAsync(command, cancellation);
            var auditEntry = command.ToDomainModel();

            await dbContext.AuditEntries.AddAsync(auditEntry, cancellation);
            await dbContext.SaveChangesAsync(cancellation);
            return new AuditEntryDto(auditEntry);
        }
    }
}