namespace EngineBay.Auditing.Tests
{
    using EngineBay.Auditing;
    using EngineBay.Persistence;
    using Microsoft.EntityFrameworkCore;
    using System.Security.Claims;

    public class BaseAuditQueryTest
    {
        private bool isDisposed;

        protected MockApplicationUser ApplicationUser { get; set; }

        protected AuditingDbContext AuditingDbContext { get; set; }

        protected ClaimsPrincipal ClaimsPrincipal { get; set; }

        protected BaseAuditQueryTest(string databaseName)
        {
            var auditingDbContextOptions = new DbContextOptionsBuilder<ModuleWriteDbContext>()
                .UseInMemoryDatabase(databaseName)
                .EnableSensitiveDataLogging()
                .Options;

            this.AuditingDbContext = new AuditingDbContext(auditingDbContextOptions);
            this.AuditingDbContext.Database.EnsureCreated();

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
                this.AuditingDbContext.Database.EnsureDeleted();
                this.AuditingDbContext.Dispose();
            }

            this.isDisposed = true;
        }
    }
}
