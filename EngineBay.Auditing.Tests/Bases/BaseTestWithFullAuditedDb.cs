namespace EngineBay.Auditing.Tests
{
    using System;
    using EngineBay.Core;
    using EngineBay.Persistence;
    using Microsoft.EntityFrameworkCore;
    using NSubstitute;

    public class BaseTestWithFullAuditedDb<TContext> : BaseTestWithDbContext<AuditingWriteDbContext>
        where TContext : ModuleDbContext
    {
        private bool isDisposed;

        public BaseTestWithFullAuditedDb(string databaseName)
            : base(databaseName + "AuditDb")
        {
            this.AuditDbContext = base.DbContext;

            var dbContextOptions = new DbContextOptionsBuilder<ModuleWriteDbContext>()
                    .UseInMemoryDatabase(databaseName)
                    .EnableSensitiveDataLogging()
                    .Options;

            this.CurrentIdentity = new FakeUserIdentity();
            var auditingInterceptor = new AuditingInterceptor(this.CurrentIdentity, this.AuditDbContext);
            var auditableModelInterceptor = new AuditableModelInterceptor(this.CurrentIdentity);

            if (Activator.CreateInstance(typeof(TContext), dbContextOptions, auditingInterceptor, auditableModelInterceptor) is not TContext context)
            {
                throw new ArgumentException("Context not created!");
            }

            this.DbContext = context;
        }

        protected new TContext DbContext { get; set; }

        protected AuditingWriteDbContext AuditDbContext { get; set; }

        protected ICurrentIdentity CurrentIdentity { get; set; }

        protected void ResetAuditEntries()
        {
            this.AuditDbContext.AuditEntries.RemoveRange(this.AuditDbContext.AuditEntries);
            this.AuditDbContext.SaveChanges();
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (this.isDisposed)
            {
                return;
            }

            if (disposing)
            {
                // free managed resources
                this.DbContext.Database.EnsureDeleted();
                this.DbContext.Dispose();

                base.Dispose(disposing);
            }

            this.isDisposed = true;
        }
    }
}
