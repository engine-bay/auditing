namespace EngineBay.Auditing.Tests
{
    using System;
    using EngineBay.Authentication;
    using EngineBay.Core;
    using EngineBay.Persistence;
    using Microsoft.EntityFrameworkCore;

    public class BaseTestWithFullAuditedDb<TContext> : BaseTestWithDbContext<AuditingWriteDbContext>
        where TContext : ModuleDbContext
    {
        public BaseTestWithFullAuditedDb(string databaseName)
            : base(databaseName + "AuditDb")
        {
            this.AuditDbContext = base.DbContext;

            var dbContextOptions = new DbContextOptionsBuilder<ModuleWriteDbContext>()
                    .UseInMemoryDatabase(nameof(AuditInterceptorTests))
                    .EnableSensitiveDataLogging()
                    .Options;

            this.currentIdentity = new SystemUserIdentity();
            var interceptor = new AuditingInterceptor(currentIdentity, this.AuditDbContext);

            var context = Activator.CreateInstance(typeof(TContext), dbContextOptions, interceptor) as TContext;
            ArgumentNullException.ThrowIfNull(context);

            this.DbContext = context;
            this.DbContext.Database.EnsureDeleted();
            this.DbContext.Database.EnsureCreated();
        }

        protected new TContext DbContext { get; set; }

        protected AuditingWriteDbContext AuditDbContext { get; set; }

        protected ICurrentIdentity currentIdentity { get; set; }

        protected void ResetAuditEntries()
        {
            this.AuditDbContext.AuditEntries.RemoveRange(this.AuditDbContext.AuditEntries);
            this.AuditDbContext.SaveChanges();
        }
    }
}
