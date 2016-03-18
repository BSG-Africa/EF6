namespace Bsg.Ef6.Utils
{
    using Context;
    using Domain;
    using Mapping;

    public interface IBulkInserterFactory
    {
        IBulkInserter<TEntity> BuildInserter<TEntity, TContext>(
            TableMapping<TEntity, TContext> mapping,
            IContextTransaction contextTransaction,
            int bufferSize,
            int timeout,
            bool preInitialise)
            where TEntity : class, IEntity<TContext>, new()
            where TContext : IDbContext;
    }
}