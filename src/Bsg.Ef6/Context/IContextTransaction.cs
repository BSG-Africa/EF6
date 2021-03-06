﻿namespace Bsg.Ef6.Context
{
    using System;
    using System.Data.Common;

    public interface IContextTransaction : IDisposable
    {
        TTransactionType UnderlyingTransaction<TTransactionType>() 
            where TTransactionType : DbTransaction;

        void Rollback();

        void Commit();
    }
}
