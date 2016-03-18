namespace Bsg.Ef6.Timeout
{
    using Context;

    public interface ITimeoutCacheService
    {
        int ContextTimeout<TContext>()
            where TContext : IDbContext;

        int BulkInsertTimeout<TContext>()
            where TContext : IDbContext;

        int BulkUpdateTimeout<TContext>()
            where TContext : IDbContext;

        void StoreTimeouts<TContext>(ContextTimeouts timeouts)
            where TContext : IDbContext;
    }
}