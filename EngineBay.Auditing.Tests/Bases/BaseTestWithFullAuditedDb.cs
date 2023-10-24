namespace EngineBay.Auditing.Tests
{
    using System;
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

            this.currentIdentity = new FakeUserIdentity();
            var interceptor = new AuditingInterceptor(this.currentIdentity, this.AuditDbContext);

            if (Activator.CreateInstance(typeof(TContext), dbContextOptions, interceptor) is not TContext context)
            {
                throw new ArgumentException("Context not created!");
            }

            this.DbContext = context;
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
