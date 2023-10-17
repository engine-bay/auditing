namespace EngineBay.Auditing.Tests
{
    using EngineBay.Persistence;
    using Microsoft.EntityFrameworkCore;

    public class BaseTestWithDbContext<TContext>
        where TContext : ModuleDbContext
    {
        private bool isDisposed;

        protected BaseTestWithDbContext(string databaseName)
        {
            var dbContextOptions = new DbContextOptionsBuilder<ModuleWriteDbContext>()
                    .UseInMemoryDatabase(databaseName)
                    .EnableSensitiveDataLogging()
                    .Options;

            var context = Activator.CreateInstance(typeof(TContext), dbContextOptions) as TContext;
            ArgumentNullException.ThrowIfNull(context);

            this.DbContext = context;
            this.DbContext.Database.EnsureDeleted();
            this.DbContext.Database.EnsureCreated();
        }

        protected TContext DbContext { get; set; }

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
