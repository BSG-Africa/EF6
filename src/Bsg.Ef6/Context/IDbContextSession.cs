namespace Bsg.Ef6.Context
{
    using System;
    using System.Linq;
    using Domain;

    public interface IDbContextSession<TContext> : IDisposable
        where TContext : IDbContext
    {
        void CommitChanges();

        void RevertChanges();

        bool HasChanges();

        void Add<TEntity>(TEntity item) 
            where TEntity : class, IEntity<TContext>, new();

        IQueryable<TEntity> All<TEntity>() 
            where TEntity : class, IEntity<TContext>, new();

        void Delete<TEntity>(TEntity item) 
            where TEntity : class, IEntity<TContext>, new();

        int ExecuteDirectNonQuery(string nonQuerySql, object[] parameters);

        IContextTransaction StartNewTransaction();
    }
}