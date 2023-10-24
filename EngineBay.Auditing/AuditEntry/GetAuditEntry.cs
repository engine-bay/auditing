namespace EngineBay.Auditing
{

    using EngineBay.Core;
    using LinqKit;
    using Microsoft.EntityFrameworkCore;
    using System.Linq.Expressions;

    public class GetAuditEntry : IQueryHandler<GetAuditEntryRequest, AuditEntryDto>
    {

        private readonly AuditingDbContext dbContext;

        public GetAuditEntry(AuditingDbContext auditingDb)
        {
            this.dbContext = auditingDb;
        }

        public async Task<AuditEntryDto> Handle(GetAuditEntryRequest queryParameters, CancellationToken cancellation)
        {
            if (queryParameters is null)
            {
                throw new ArgumentNullException(nameof(queryParameters));
            }

            Expression<Func<AuditEntry, bool>> filterPredicate = auditEntry => auditEntry.Id == queryParameters.Id;

            var auditEntry = await dbContext.AuditEntries
              .Where(filterPredicate)
              .Select(auditEntry => new AuditEntryDto(auditEntry))
              .AsExpandable()
              .FirstOrDefaultAsync(cancellation)
              ;

            if (auditEntry == null)
            {
                throw new NotFoundException("No audit entry was found");
            }

            return auditEntry;
        }
    }
}
