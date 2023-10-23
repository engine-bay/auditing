namespace EngineBay.Auditing
{
    using EngineBay.Core;
    using FluentValidation;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using System;

    public class AuditingModule : IModule
    {
        public IServiceCollection RegisterModule(IServiceCollection services, IConfiguration configuration)
        {
            // Register commands
            services.AddTransient<CreateAuditEntry>();

            // Register queries
            services.AddTransient<GetAuditEntry>();
            services.AddTransient<QueryAuditEntries>();

            // Register validators
            services.AddTransient<IValidator<CreateAuditEntryRequest>, CreateAuditEntryValidator>();

            services.AddTransient<AuditingInterceptor>();

            return services;
        }

        public RouteGroupBuilder MapEndpoints(RouteGroupBuilder endpoints)
        {
            AuditEntryEndpoints.MapEndpoints(endpoints);
            return endpoints;
        }

        public WebApplication AddMiddleware(WebApplication app)
        {
            return app;
        }

        public IServiceCollection RegisterPolicies(IServiceCollection services)
        {
            return services;
        }

        public void SeedDatabase(string seedDataPath, IServiceProvider serviceProvider)
        {
            return;
        }
    }
}