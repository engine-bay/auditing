using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EngineBay.Auditing
{
    public interface IAuditingInterceptor : ISaveChangesInterceptor
    {
    }
}
