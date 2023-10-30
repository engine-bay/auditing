# EngineBay.Auditing

[![NuGet version](https://badge.fury.io/nu/EngineBay.Auditing.svg)](https://badge.fury.io/nu/EngineBay.Auditing)

[//]: # ([![Maintainability]&#40;&#41;]&#40;https://codeclimate.com/github/engine-bay/auditing/maintainability&#41;)

[//]: # ([![Test Coverage]&#40;&#41;]&#40;https://codeclimate.com/github/engine-bay/auditing/test_coverage&#41;)

Auditing module for EngineBay published to [EngineBay.Auditing](https://www.nuget.org/packages/EngineBay.Auditing/) on NuGet.

## About

This module introduces an [auditing interceptor](EngineBay.Auditing/Interceptors/AuditingInterceptor.cs) and the concept of an [audit entry](EngineBay.Auditing/Models/AuditEntry.cs). The purpose is to provide an easily-integrated simple auditing solution, with space for custom implementations or usages. This module makes use of dependency injection to offload the concrete implementations of several classes to modules more suited to their functionality.

The auditing interceptor will maintain an [auditing DbContext](EngineBay.Auditing/Persistence/AuditingWriteDbContext.cs) that is separate from any other DbContexts that your application might use. 

While [two API endpoints are exposed](EngineBay.Auditing/AuditEntry/AuditEntryEndpoints.cs) to view existing audit entries, creating audit entries is not and must not be possible via network requests. However, a command handler to [create audit entries](EngineBay.Auditing/AuditEntry/CreateAuditEntry.cs) is still present so that developers can programmatically create audit entries if they have models that they wish to be audited in a non-standard manner (such as excluding certain fields from the change log or having a custom changes string).  

Be aware that EngineBay.Core provides only the interface ICurrentIdentity. The auditing interceptor will need an implementation (such as the ones in EngineBay.Authentication) in order to find the identity of the user who is triggering an audit.

## Usage

In order to use the interceptor, you will need to register it on your own DbContext using `DbContextOptionsBuilder.AddInterceptors()`. For example:

```cs
namespace EngineBay.Blueprints
{
    using EngineBay.Auditing;
    using EngineBay.Persistence;
    using Microsoft.EntityFrameworkCore;

    public class BlueprintsWriteDbContext : BlueprintsQueryDbContext
    {
        private readonly IAuditingInterceptor auditingInterceptor;

        public BlueprintsWriteDbContext(DbContextOptions<ModuleWriteDbContext> options, IAuditingInterceptor auditingInterceptor)
            : base(options)
        {
            this.auditingInterceptor = auditingInterceptor;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (optionsBuilder is null)
            {
                throw new ArgumentNullException(nameof(optionsBuilder));
            }

            optionsBuilder.AddInterceptors(this.auditingInterceptor);

            base.OnConfiguring(optionsBuilder);
        }
    }
}
```

The interceptor will trigger anytime you save changes to a DbSet from this DbContext, but it will only create audit logs for models implementing the [AuditableModel](https://github.com/engine-bay/persistence/blob/main/EngineBay.Persistence/Models/AuditableModel.cs) class. Remember to include the added fields in your CreateDataAnnotations method for Entity Framework. 