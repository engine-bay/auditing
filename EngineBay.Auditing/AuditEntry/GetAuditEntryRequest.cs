namespace EngineBay.Auditing
{

    using System.Security.Claims;

    public class GetAuditEntryRequest
    {
        public GetAuditEntryRequest(Guid id)
        {
            this.Id = id;
        }

        public Guid Id { get; set; }
    }
}