using EngineBay.Core;
using System.Security.Claims;

namespace EngineBay.Auditing
{
    public class QueryAuditEntriesRequest
    {
        public QueryAuditEntriesRequest(ClaimsPrincipal claimsPrincipal, PaginationParameters paginationParameters)
        {
            this.ClaimsPrincipal = claimsPrincipal;
            this.PaginationParameters = paginationParameters;
        }

        public PaginationParameters PaginationParameters { get; set; }

        public ClaimsPrincipal ClaimsPrincipal { get; set; }
    }
}