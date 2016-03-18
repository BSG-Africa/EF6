namespace Bsg.Ef6.Repo
{
    using Context;
    using Domain;
    using Mapping;
    using Timeout;
    using Utils;

    public class GenericRepository<TEntity, TContext> : BulkInsertRepository<TEntity, TContext>, IGenericRepository<TEntity, TContext> 
        where TEntity : class, IEntity<TContext>, new()
        where TContext : IDbContext
    {
        public GenericRepository(
            IDbContextSession<TContext> session, 
            ITimeoutCacheService timeoutCacheService, 
            IBulkInserterFactory bulkInserterFactory,
            ITableMappingsCacheService tableMappingsCacheService)
            : base(session, timeoutCacheService, bulkInserterFactory, tableMappingsCacheService)
        {
        }
    }
}