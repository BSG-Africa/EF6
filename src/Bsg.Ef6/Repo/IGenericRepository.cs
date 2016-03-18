namespace Bsg.Ef6.Repo
{
    using Context;
    using Domain;

    public interface IGenericRepository<TEntity, TContext> : IBulkInsertRepository<TEntity, TContext>
        where TEntity : class, IEntity<TContext>, new()
        where TContext : IDbContext
    {
    }
}