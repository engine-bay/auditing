namespace EngineBay.Auditing
{
    using EngineBay.Persistence;
    using Microsoft.EntityFrameworkCore;

    public class AuditingWriteDbContext : AuditingQueryDbContext
    {
        public AuditingWriteDbContext(DbContextOptions<ModuleWriteDbContext> options) : base(options)
        {
        }
    }
}