namespace EngineBay.Auditing
{

    using EngineBay.Core;
    using EngineBay.Persistence;
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

        public async Task<AuditEntryDto> Handle(GetAuditEntryRequest getAuditEntryRequest, CancellationToken cancellation)
        {

            if (getAuditEntryRequest?.ClaimsPrincipal == null)
            {
                throw new ArgumentNullException(nameof(getAuditEntryRequest));
            }
            Expression<Func<AuditEntry, bool>> filterPredicate = auditEntry => auditEntry.Id == getAuditEntryRequest.Id;

            var auditEntry = await this.dbContext.AuditEntries
              .Where(filterPredicate)
              .Select(auditEntry => new AuditEntryDto(auditEntry))
              .AsExpandable()
              .FirstOrDefaultAsync(cancellation);

            if (auditEntry == null)
            {
                throw new NotFoundException("No audit entry was found");
            }

            return auditEntry;
        }
    }
}
