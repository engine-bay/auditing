using System.Globalization;

namespace EngineBay.Auditing
{
    using EngineBay.Core;
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

        public async Task<PaginatedDto<AuditEntryDto>> Handle(QueryAuditEntriesRequest queryParameters, CancellationToken cancellation)
        {
            if (queryParameters?.PaginationParameters is null)
            {
                throw new ArgumentNullException(nameof(queryParameters));
            }

            var query = this.dbContext.AuditEntries.AsExpandable();

            var limit = queryParameters.PaginationParameters.Limit;
            var skip = limit > 0 ? queryParameters.PaginationParameters.Skip : 0;

            var total = await query.CountAsync(cancellation).ConfigureAwait(false);
            var format = new DateTimeFormatInfo();

            Expression<Func<AuditEntry, string?>> sortByPredicate = queryParameters.PaginationParameters.SortBy switch
            {
                nameof(AuditEntry.CreatedAt) => auditEntry => auditEntry.CreatedAt.ToString(format),
                nameof(AuditEntry.LastUpdatedAt) => auditEntry => auditEntry.LastUpdatedAt.ToString(format),
                nameof(AuditEntry.ActionType) => auditEntry => auditEntry.ActionType,
                nameof(AuditEntry.ApplicationUserId) => auditEntry => auditEntry.ApplicationUserId.ToString(),
                nameof(AuditEntry.ApplicationUserName) => auditEntry => auditEntry.ApplicationUserName,
                nameof(AuditEntry.EntityId) => auditEntry => auditEntry.EntityId,
                nameof(AuditEntry.EntityName) => auditEntry => auditEntry.EntityName,
                nameof(AuditEntry.Id) => auditEntry => auditEntry.Id.ToString(),
                _ => throw new ArgumentNullException(queryParameters.PaginationParameters.SortBy),
            };

            query = this.Sort(query, sortByPredicate, queryParameters.PaginationParameters);
            query = this.Paginate(query, queryParameters.PaginationParameters);

            var auditEntries = limit > 0 ? await query
                .ToListAsync(cancellation).ConfigureAwait(false)
              : new List<AuditEntry>();

            var auditEntryDtos = auditEntries.Select(auditEntry => new AuditEntryDto(auditEntry));
            return new PaginatedDto<AuditEntryDto>(total, skip, limit, auditEntryDtos);
        }
    }
}