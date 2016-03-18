namespace Bsg.Ef6.Timeout
{
    using System;
    using System.Collections.Generic;
    using Context;

    public class TimeoutCacheService : ITimeoutCacheService
    {
        private readonly IDictionary<Type, ContextTimeouts> timeoutCache;

        public TimeoutCacheService()
        {
            this.timeoutCache = new Dictionary<Type, ContextTimeouts>();
        }

        public int ContextTimeout<TContext>()
            where TContext : IDbContext
        {
            return this.timeoutCache[typeof(TContext)].ContextTimeout;
        }

        public int BulkInsertTimeout<TContext>()
            where TContext : IDbContext
        {
            return this.timeoutCache[typeof(TContext)].BulkInsertTimeout;
        }

        public int BulkUpdateTimeout<TContext>()
            where TContext : IDbContext
        {
            return this.timeoutCache[typeof(TContext)].BulkUpdateTimeout;
        }

        public void StoreTimeouts<TContext>(ContextTimeouts timeouts)
            where TContext : IDbContext
        {
            if (timeouts == null)
            {
                throw new ArgumentNullException(nameof(timeouts));
            }

            this.timeoutCache.Add(typeof(TContext), timeouts);
        }
    }
}