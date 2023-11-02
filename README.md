# EngineBay.Auditing

[![NuGet version](https://badge.fury.io/nu/EngineBay.Auditing.svg)](https://badge.fury.io/nu/EngineBay.Auditing)

[//]: # ([![Maintainability]&#40;&#41;]&#40;https://codeclimate.com/github/engine-bay/auditing/maintainability&#41;)

[//]: # ([![Test Coverage]&#40;&#41;]&#40;https://codeclimate.com/github/engine-bay/auditing/test_coverage&#41;)

Auditing module for EngineBay published to [EngineBay.Auditing](https://www.nuget.org/packages/EngineBay.Auditing/) on NuGet.

## About

This module introduces an [AuditingInterceptor](EngineBay.Auditing/Interceptors/AuditingInterceptor.cs) and the concept of an [AuditEntry](EngineBay.Auditing/Models/AuditEntry.cs). The purpose is to provide an easily-integrated simple auditing solution, with space for custom implementations or usages. This module makes use of dependency injection to offload the concrete implementations of several classes to modules more suited to their functionality.

The auditing interceptor will maintain an [auditing DbContext](EngineBay.Auditing/Persistence/AuditingWriteDbContext.cs) that is separate from any other DbContexts that your application might use. 

While [two API endpoints are exposed](EngineBay.Auditing/AuditEntry/AuditEntryEndpoints.cs) to view existing audit entries, creating audit entries is not and must not be possible via network requests. However, a command handler to [create audit entries](EngineBay.Auditing/AuditEntry/CreateAuditEntry.cs) is still present so that developers can programmatically create audit entries if they have models that they wish to be audited in a non-standard manner (such as excluding certain fields from the change log or having a custom changes string).  

Be aware that [EngineBay.Core](https://github.com/engine-bay/core) provides only the interface [ICurrentIdentity](https://github.com/engine-bay/core/blob/main/EngineBay.Core/Interfaces/ICurrentIdentity.cs). The auditing interceptor will need an implementation (such as the ones in EngineBay.Authentication) in order to find the identity of the user who is triggering an audit.

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

The interceptor will trigger anytime you save changes to a DbSet from this DbContext, but it will only create audit logs for models implementing the [AuditableModel](https://github.com/engine-bay/persistence/blob/main/EngineBay.Persistence/Models/AuditableModel.cs) class. Remember to include the added fields in your CreateDataAnnotations method for Entity Framework (see [ApplicationUser](https://github.com/engine-bay/persistence/blob/main/EngineBay.Persistence/Models/ApplicationUser.cs) for example). 

### Registration

This module cannot run on its own. You will need to register it and its DbContext in your application to use its functionality. See [EngineBay.CommunityEdition](https://github.com/engine-bay/engine-bay-ce)'s example of [module registration](https://github.com/engine-bay/engine-bay-ce/blob/main/EngineBay.CommunityEdition/Modules/ModuleRegistration.cs) for an example of how to do this. 

```cs
        public static IReadOnlyCollection<IModuleDbContext> GetRegisteredDbContexts(DbContextOptions<ModuleWriteDbContext> dbOptions)
        {
            var dbContexts = new List<IModuleDbContext>
            {
                new AuditingDbContext(dbOptions),
                // Other DbContexts...
            };

            return dbContexts;
        }

        private static IEnumerable<IModule> GetRegisteredModules()
        {
            var modules = new List<IModule>();

            modules.Add(new AuditingModule());
            // Other modules...

            Console.WriteLine($"Discovered {modules.Count} EngineBay modules");
            return modules;
        }
```

### Environment Variables

The following environment variables control the auditing behavior.

| Environment variable | Default value |         Options         | Description                                                                                                                                                                                                                                        |
|:---------------------|:-------------:|:-----------------------:|:---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `AUDITING_ENABLED`   |    `true`     | `true`, `false`, `none` | This can disable tracking and auditing of changes saved to the database. It is not recommended to disable this unless EngineBay is processing PII data. Disabling auditing can provide a slight performance boost if traceability is not required. |

## Dependencies

* [EngineBay.Core](https://github.com/engine-bay/core): Provides several shared classes and base interfaces.
* [EngineBay.Persistence](https://github.com/engine-bay/persistence): Provides a framework to connect to a database, as well as some classes like the AuditableModel.