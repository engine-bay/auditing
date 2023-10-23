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

        public async Task<AuditEntryDto> Handle(CreateAuditEntryRequest inputParameters, CancellationToken cancellation)
        {
            ArgumentNullException.ThrowIfNull(inputParameters);

            await this.validator.ValidateAndThrowAsync(inputParameters, cancellation).ConfigureAwait(false);
            var auditEntry = inputParameters.ToDomainModel();

            await dbContext.AuditEntries.AddAsync(auditEntry, cancellation).ConfigureAwait(false);
            await dbContext.SaveChangesAsync(cancellation).ConfigureAwait(false);
            return new AuditEntryDto(auditEntry);
        }
    }
}