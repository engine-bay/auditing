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
            if (inputParameters is null)
            {
                throw new ArgumentNullException(nameof(inputParameters));
            }

            await this.validator.ValidateAndThrowAsync(inputParameters, cancellation);
            var auditEntry = inputParameters.ToDomainModel();

            await dbContext.AuditEntries.AddAsync(auditEntry, cancellation);
            await dbContext.SaveChangesAsync(cancellation);
            return new AuditEntryDto(auditEntry);
        }
    }
}