namespace EngineBay.Auditing.Tests
{
    using EngineBay.Persistence;
    using Microsoft.EntityFrameworkCore;

    public class BaseTestWithDbContext<TContext> : IDisposable
        where TContext : ModuleDbContext
    {
        private bool isDisposed;

        protected BaseTestWithDbContext(string databaseName)
        {
            var dbContextOptions = new DbContextOptionsBuilder<ModuleWriteDbContext>()
                    .UseInMemoryDatabase(databaseName)
                    .EnableSensitiveDataLogging()
                    .Options;

            if (Activator.CreateInstance(typeof(TContext), dbContextOptions) is not TContext context)
            {
                throw new ArgumentException("Context not created!");
            }

            this.DbContext = context;
        }

        protected TContext DbContext { get; set; }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

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
