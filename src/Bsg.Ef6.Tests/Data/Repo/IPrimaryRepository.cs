namespace Bsg.Ef6.Tests.Data.Repo
{
    using Context;
    using Ef6.Domain;
    using Ef6.Repo;

    public interface IPrimaryRepository<TEntity> : IBulkInsertRepository<TEntity, PrimaryContext>
        where TEntity : class, IEntity<PrimaryContext>, new()
    {
    }
}