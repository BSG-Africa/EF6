namespace Bsg.Ef6.Context
{
    using System;
    using System.Data.Entity;
    using System.Linq;
    using Domain;

    public class DbContextSession<TContext> : IDbContextSession<TContext>
        where TContext : IDbContext
    {
        #region Private Fields

        private readonly IDbContextFactory contextFactory;
        
        private readonly object instanceInstantiateLockObj;
        
        private IDbContext contextInstance;

        private bool contextInstantiated;

        #endregion

        #region Constructors

        public DbContextSession(IDbContextFactory contextFactory)
        {
            this.instanceInstantiateLockObj = new object();
            this.contextFactory = contextFactory;
        }

        #endregion

        #region Public Properties
        public IDbContext SafeContext
        {
            get
            {
                if (!this.contextInstantiated)
                {
                    lock (this.instanceInstantiateLockObj)
                    {
                        if (!this.contextInstantiated)
                        {
                            this.InstantiateContext();
                        }
                    }
                }
                
                return this.contextInstance;
            }
        }
        #endregion

        #region Creates

        public void Add<TEntity>(TEntity item) 
            where TEntity : class, IEntity<TContext>, new()
        {
            this.SafeContext.EntitySet<TEntity, TContext>().Add(item);
        }

        #endregion

        #region Retrieves

        public IQueryable<TEntity> All<TEntity>() 
            where TEntity : class, IEntity<TContext>, new()
        {
            return this.SafeContext.EntitySet<TEntity, TContext>().AsQueryable();
        }

        #endregion

        #region Deletes

        public void Delete<TEntity>(TEntity item)
            where TEntity : class, IEntity<TContext>, new()
        {
            this.SafeContext.EntitySet<TEntity, TContext>().Remove(item);
        }

        #endregion

        #region Other Public Methods

        public int ExecuteDirectNonQuery(string nonQuerySql, object[] parameters)
        {
            return this.SafeContext.Database.ExecuteSqlCommand(TransactionalBehavior.EnsureTransaction, nonQuerySql, parameters);
        }

        public IContextTransaction StartNewTransaction()
        {
            return new ContextTransaction(this.SafeContext.Database.BeginTransaction());
        }

        public void CommitChanges()
        {
            this.SafeContext.SaveChanges();
        }

        public void RevertChanges()
        {
            this.DisposeContext();
        }

        public bool HasChanges()
        {
            return this.contextInstantiated && this.SafeContext.IsDirty();
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.DisposeContext();
            }
        }

        #endregion

        #region Private Methods

        private void InstantiateContext()
        {
            this.contextInstance = this.contextFactory.BuildContext<TContext>();
            this.contextInstantiated = true;
        }

        private void DisposeContext()
        {
            if (this.contextInstantiated && this.contextInstance != null)
            {
                this.contextInstance.Dispose();
                this.contextInstance = null;
                this.contextInstantiated = false;
            }
        }
        #endregion
    }
}