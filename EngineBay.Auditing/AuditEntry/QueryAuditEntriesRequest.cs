using EngineBay.Core;
using System.Security.Claims;

namespace EngineBay.Auditing
{
    public class QueryAuditEntriesRequest
    {
        public QueryAuditEntriesRequest(PaginationParameters paginationParameters)
        {
            this.PaginationParameters = paginationParameters;
        }

        public PaginationParameters PaginationParameters { get; set; }
    }
}