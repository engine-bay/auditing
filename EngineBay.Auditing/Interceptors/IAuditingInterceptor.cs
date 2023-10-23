namespace EngineBay.Auditing
{
    using Microsoft.EntityFrameworkCore.Diagnostics;

    public interface IAuditingInterceptor : ISaveChangesInterceptor
    {
    }
}
