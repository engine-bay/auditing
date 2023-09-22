namespace EngineBay.Auditing
{
    using EngineBay.Persistence;
    using Microsoft.EntityFrameworkCore;

    public class AuditingDbContext : ModuleWriteDbContext
    {
        public AuditingDbContext(DbContextOptions<ModuleWriteDbContext> options) : base(options)
        {
        }

        // TODO: Why are we overriding this?
        /// <inheritdoc/>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}