namespace EngineBay.Auditing.Tests
{
    using EngineBay.Persistence;
    using Microsoft.EntityFrameworkCore;
    using System.Security.Claims;

    public class BaseAuditCommandTest : IDisposable
    {
        private bool isDisposed;

        protected AuditingWriteDbContext AuditingWriteDbContext { get; set; }

        protected MockApplicationUser ApplicationUser { get; set; }

        protected ClaimsPrincipal ClaimsPrincipal { get; set; }

        protected BaseAuditCommandTest(string databaseName)
        {
            var auditingDbContextOptions = new DbContextOptionsBuilder<ModuleWriteDbContext>()
                .UseInMemoryDatabase(databaseName)
                .EnableSensitiveDataLogging()
                .Options;

            this.AuditingWriteDbContext = new AuditingWriteDbContext(auditingDbContextOptions);
            this.AuditingWriteDbContext.Database.EnsureDeleted();
            this.AuditingWriteDbContext.Database.EnsureCreated();

            var persistenceDbContextOptions = new DbContextOptionsBuilder<ModuleWriteDbContext>()
                .UseInMemoryDatabase(databaseName + "_persistence")
                .EnableSensitiveDataLogging()
                .Options;

            this.ApplicationUser = new MockApplicationUser();

            var claims = new List<Claim>()
            {
                new Claim(System.Security.Claims.ClaimTypes.NameIdentifier, this.ApplicationUser.Id.ToString() ?? "tests"),
            };

            var identity = new ClaimsIdentity(claims, "TestAuthType");
            this.ClaimsPrincipal = new ClaimsPrincipal(identity);
        }

        // Dispose() calls Dispose(true)
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

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
                this.AuditingWriteDbContext.Database.EnsureDeleted();
                this.AuditingWriteDbContext.Dispose();
            }

            this.isDisposed = true;
        }
    }
}
