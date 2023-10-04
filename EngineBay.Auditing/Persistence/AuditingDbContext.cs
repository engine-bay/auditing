namespace EngineBay.Auditing
{
    using EngineBay.Persistence;
    using Microsoft.EntityFrameworkCore;

    public class AuditingDbContext : ModuleWriteDbContext
    {
        public AuditingDbContext(DbContextOptions<ModuleWriteDbContext> options) : base(options)
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