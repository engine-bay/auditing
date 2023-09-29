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

        protected ClaimsPrincipal ClaimsPrincipal { get; set; }

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

            var claims = new List<Claim>()
            {
                new Claim(System.Security.Claims.ClaimTypes.NameIdentifier, this.ApplicationUser.Id.ToString() ?? "tests"),
            };

            var identity = new ClaimsIdentity(claims, "TestAuthType");
            this.ClaimsPrincipal = new ClaimsPrincipal(identity);
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
