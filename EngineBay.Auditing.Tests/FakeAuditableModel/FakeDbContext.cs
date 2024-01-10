namespace EngineBay.Auditing.Tests.FakeAuditableModel
{
    using EngineBay.Persistence;
    using Microsoft.EntityFrameworkCore;

    public class FakeDbContext : ModuleWriteDbContext
    {
        private readonly IAuditingInterceptor databaseAuditingInterceptor;

        private readonly AuditableModelInterceptor auditableModelInterceptor;

        public FakeDbContext(
            DbContextOptions<ModuleWriteDbContext> options,
            IAuditingInterceptor databaseAuditingInterceptor,
            AuditableModelInterceptor auditableModelInterceptor)
            : base(options)
        {
            this.databaseAuditingInterceptor = databaseAuditingInterceptor;
            this.auditableModelInterceptor = auditableModelInterceptor;
        }

        public virtual DbSet<FakeModel> FakeModels { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (optionsBuilder is null)
            {
                throw new ArgumentNullException(nameof(optionsBuilder));
            }

            optionsBuilder.AddInterceptors(this.auditableModelInterceptor);
            optionsBuilder.AddInterceptors(this.databaseAuditingInterceptor);

            base.OnConfiguring(optionsBuilder);
        }
    }
}
