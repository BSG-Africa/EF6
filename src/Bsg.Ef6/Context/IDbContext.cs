namespace Bsg.Ef6.Context
{
    using System;
    using System.Data.Entity;
    using Domain;
    using Mapping;

    public interface IDbContext : IDisposable
    {
        Database Database { get; }

        IDbSet<TEntity> EntitySet<TEntity, TContext>()
            where TEntity : class, IEntity<TContext>, new()
            where TContext : IDbContext;

        bool IsDirty();

        int SaveChanges();

        void SetLogger(Action<string> logAction);

        void SetCommandTimeout(int timeout);

        void BindPreGeneratedViews();

        ContextTableMappings GetMappings<TContext>()
            where TContext : IDbContext;
    }
}
