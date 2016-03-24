namespace Bsg.Ef6.Tests.Services
{
    using System.Linq;
    using Context;
    using Data.Context;
    using Data.Domain;
    using Mapping;
    using Repo;
    using Timeout;
    using Utils;

    public class OneRepository : BulkInsertRepository<One, SecondaryContext>, IOneRepository
    {
        public OneRepository(
            IDbContextSession<SecondaryContext> session,
            ITimeoutCacheService timeoutCacheService,
            IBulkInserterFactory bulkInserterFactory,
            ITableMappingsCacheService tableMappingsCacheService)
            : base(session, timeoutCacheService, bulkInserterFactory, tableMappingsCacheService)
        {
        }

        public IQueryable<One> AllJohns()
        {
            return this.FindAll(e => e.Name.ToLower() == "john");
        } 
    }
}
