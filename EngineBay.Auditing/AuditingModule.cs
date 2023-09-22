namespace EngineBay.Auditing
{
    using EngineBay.Core;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    public class AuditingModule : IModule
    {
        public IServiceCollection RegisterModule(IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<HttpContextWrapper>();

            return services;
        }

        public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
        {
            throw new NotImplementedException();
        }

        public WebApplication AddMiddleware(WebApplication app)
        {
            throw new NotImplementedException();
        }
    }
}