namespace Bsg.Ef6.Context
{
    using System;
    using System.Data.Common;
    using System.Data.Entity;

    public class ContextTransaction : IContextTransaction
    {
        private readonly DbContextTransaction dbContextTransaction;

        public ContextTransaction(DbContextTransaction dbContextTransaction)
        {
            this.dbContextTransaction = dbContextTransaction;
        }

        public TTransactionType UnderlyingTransaction<TTransactionType>()
            where TTransactionType : DbTransaction
        {
            return this.dbContextTransaction.UnderlyingTransaction as TTransactionType;
        }

        public void Rollback()
        {
            this.dbContextTransaction.Rollback();
        }

        public void Commit()
        {
            this.dbContextTransaction.Commit();
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
                this.dbContextTransaction.Dispose();
            }
        }
    }
}
