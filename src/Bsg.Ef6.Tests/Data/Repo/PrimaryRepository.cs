namespace Bsg.Ef6.Tests.Data.Repo
{
    using Context;
    using Ef6.Context;
    using Ef6.Domain;
    using Ef6.Repo;
    using Mapping;
    using Timeout;
    using Utils;

    public class PrimaryRepository<TEntity> : BulkInsertRepository<TEntity, PrimaryContext>, IPrimaryRepository<TEntity>
        where TEntity : class, IEntity<PrimaryContext>, new()
    {
        public PrimaryRepository(
            IDbContextSession<PrimaryContext> session,
            ITimeoutCacheService timeoutCacheService,
            IBulkInserterFactory bulkInserterFactory,
            ITableMappingsCacheService tableMappingsCacheService)
            : base(session, timeoutCacheService, bulkInserterFactory, tableMappingsCacheService)
        {
        }
    }
}
