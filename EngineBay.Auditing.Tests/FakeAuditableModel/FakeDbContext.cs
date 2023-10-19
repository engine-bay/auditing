namespace EngineBay.Auditing.Tests.FakeAuditableModel
{
    using EngineBay.Persistence;
    using Microsoft.EntityFrameworkCore;

    public class FakeDbContext : ModuleWriteDbContext
    {
        private readonly IAuditingInterceptor databaseAuditingInterceptor;

        public FakeDbContext(DbContextOptions<ModuleWriteDbContext> options, IAuditingInterceptor databaseAuditingInterceptor)
            : base(options)
        {
            this.databaseAuditingInterceptor = databaseAuditingInterceptor;
        }

        public virtual DbSet<FakeModel> FakeModels { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            ArgumentNullException.ThrowIfNull(optionsBuilder);

            optionsBuilder.AddInterceptors(this.databaseAuditingInterceptor);

            base.OnConfiguring(optionsBuilder);
        }
    }
}
