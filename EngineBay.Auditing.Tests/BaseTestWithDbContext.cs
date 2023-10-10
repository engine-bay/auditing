namespace EngineBay.Auditing.Tests
{
    using System.Security.Claims;
    using EngineBay.Persistence;
    using Microsoft.EntityFrameworkCore;

    public class BaseTestWithDbContext<TContext>
        where TContext : ModuleDbContext
    {
        private bool isDisposed;

        protected MockApplicationUser ApplicationUser { get; set; }

        protected TContext DbContext { get; set; }

        protected BaseTestWithDbContext(string databaseName)
        {
            var auditingDbContextOptions = new DbContextOptionsBuilder<ModuleWriteDbContext>()
                .UseInMemoryDatabase(databaseName)
                .EnableSensitiveDataLogging()
                .Options;

            var context = Activator.CreateInstance(typeof(TContext), auditingDbContextOptions) as TContext;
            ArgumentNullException.ThrowIfNull(context, nameof(context));

            this.DbContext = context;
            this.DbContext.Database.EnsureDeleted();
            this.DbContext.Database.EnsureCreated();

            this.ApplicationUser = new MockApplicationUser();
        }

        /// <inheritdoc/>
        // Dispose() calls Dispose(true)
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc/>
        // The bulk of the clean-up code is implemented in Dispose(bool)
        protected virtual void Dispose(bool disposing)
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
            }

            this.isDisposed = true;
        }
    }
}
