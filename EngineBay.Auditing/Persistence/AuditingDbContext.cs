namespace EngineBay.Auditing
{
    using EngineBay.Persistence;
    using Microsoft.EntityFrameworkCore;

    public class AuditingDbContext : ModuleWriteDbContext
    {
        public AuditingDbContext(DbContextOptions<ModuleWriteDbContext> options, TimestampInterceptor timestampInterceptor) : base(options, timestampInterceptor)
        {
        }

        public virtual DbSet<AuditEntry> AuditEntries { get; set; } = null!;

        /// <inheritdoc/>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            AuditEntry.CreateDataAnnotations(modelBuilder);

            base.OnModelCreating(modelBuilder);
        }
    }
}