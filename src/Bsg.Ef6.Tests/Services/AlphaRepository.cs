namespace Bsg.Ef6.Tests.Services
{
    using System.Linq;
    using Context;
    using Data.Context;
    using Data.Domain;
    using Data.Repo;
    using Mapping;
    using Timeout;
    using Utils;

    public class AlphaRepository : PrimaryRepository<Alpha>, IAlphaRepository
    {
        public AlphaRepository(
            IDbContextSession<PrimaryContext> session,
            ITimeoutCacheService timeoutCacheService,
            IBulkInserterFactory bulkInserterFactory,
            ITableMappingsCacheService tableMappingsCacheService)
            : base(session, timeoutCacheService, bulkInserterFactory, tableMappingsCacheService)
        {
        }

        public IQueryable<Alpha> Active()
        {
            return this.FindAll(e => e.IsActive);
        }
    }
}