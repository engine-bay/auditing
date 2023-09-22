namespace EngineBay.Auditing
{

    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Primitives;

    public class HttpContextWrapper
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        public HttpContextWrapper(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;

            // TODO: use the claims principal to get the current identity
            Username = httpContextAccessor.HttpContext?.User?.Identity?.Name;

            var values = new StringValues();
            httpContextAccessor.HttpContext?.Request.Headers.TryGetValue("User-ID", out values);
            UserId = Guid.Parse(values.First() ?? "");
        }

        public string? Username { get; }

        public Guid UserId { get; }

    }
}
