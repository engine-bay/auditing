namespace EngineBay.Auditing
{
    using EngineBay.Persistence;
    using Microsoft.EntityFrameworkCore;

    public class AuditingQueryDbContext : AuditingDbContext
    {
        public AuditingQueryDbContext(DbContextOptions<ModuleWriteDbContext> options) : base(options)
        {
        }
    }
}
