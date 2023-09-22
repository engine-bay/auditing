namespace EngineBay.Auditing
{

    using System.Security.Claims;

    public class GetAuditEntryRequest
    {
        public GetAuditEntryRequest(ClaimsPrincipal claimsPrincipal, Guid id)
        {
            this.ClaimsPrincipal = claimsPrincipal;
            this.Id = id;
        }

        public Guid Id { get; set; }

        public ClaimsPrincipal ClaimsPrincipal { get; set; }
    }
}