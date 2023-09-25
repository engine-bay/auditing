namespace EngineBay.Auditing
{

    using EngineBay.Core;
    using EngineBay.Persistence;
    using LinqKit;
    using Microsoft.EntityFrameworkCore;
    using System.Linq.Expressions;

    public class QueryAuditEntries : PaginatedQuery<AuditEntry>, IQueryHandler<QueryAuditEntriesRequest, PaginatedDto<AuditEntryDto>>
    {
        private readonly AuditingDbContext dbContext;

        public QueryAuditEntries(AuditingDbContext auditingDb)
        {
            this.dbContext = auditingDb;
        }

        public async Task<PaginatedDto<AuditEntryDto>> Handle(QueryAuditEntriesRequest queryAuditEntriesRequest, CancellationToken cancellation)
        {
            if (queryAuditEntriesRequest?.PaginationParameters is null)
            {
                throw new ArgumentNullException(nameof(queryAuditEntriesRequest));
            }

            var query = this.dbContext.AuditEntries.AsExpandable();

            var limit = queryAuditEntriesRequest.PaginationParameters.Limit;
            var skip = limit > 0 ? queryAuditEntriesRequest.PaginationParameters.Skip : 0;

            var total = await query.CountAsync(cancellation);

            Expression<Func<AuditEntry, string?>> sortByPredicate = queryAuditEntriesRequest.PaginationParameters.SortBy switch
            {
                nameof(AuditEntry.CreatedAt) => auditEntry => auditEntry.CreatedAt.ToString(),
                nameof(AuditEntry.LastUpdatedAt) => auditEntry => auditEntry.LastUpdatedAt.ToString(),
                nameof(AuditEntry.ActionType) => auditEntry => auditEntry.ActionType,
                nameof(AuditEntry.ApplicationUserId) => auditEntry => auditEntry.ApplicationUserId.ToString(),
                nameof(AuditEntry.ApplicationUserName) => auditEntry => auditEntry.ApplicationUserName,
                nameof(AuditEntry.EntityId) => auditEntry => auditEntry.EntityId,
                nameof(AuditEntry.EntityName) => auditEntry => auditEntry.EntityName,
                nameof(AuditEntry.Id) => auditEntry => auditEntry.Id.ToString(),
                _ => throw new ArgumentNullException(queryAuditEntriesRequest.PaginationParameters.SortBy),
            };

            query = this.Sort(query, sortByPredicate, queryAuditEntriesRequest.PaginationParameters);
            query = this.Paginate(query, queryAuditEntriesRequest.PaginationParameters);

            var auditEntries = limit > 0 ? await query
                .ToListAsync(cancellation)
              : new List<AuditEntry>();

            var auditEntryDtos = auditEntries.Select(auditEntry => new AuditEntryDto(auditEntry));
            return new PaginatedDto<AuditEntryDto>(total, skip, limit, auditEntryDtos);
        }
    }
}