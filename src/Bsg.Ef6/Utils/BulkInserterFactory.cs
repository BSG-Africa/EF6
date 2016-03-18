﻿namespace Bsg.Ef6.Utils
{
    using Context;
    using Domain;
    using Mapping;

    public class BulkInserterFactory : IBulkInserterFactory
    {
        public IBulkInserter<TEntity> BuildInserter<TEntity, TContext>(
            TableMapping<TEntity, TContext> mapping,
            IContextTransaction contextTransaction,
            int bufferSize,
            int timeout,
            bool preInitialise) 
            where TEntity : class, IEntity<TContext>, new()
            where TContext : IDbContext
        {
            return new BulkInserter<TEntity, TContext>(mapping, contextTransaction, bufferSize, timeout, preInitialise);
        }
    }
}