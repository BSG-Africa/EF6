namespace Bsg.Ef6.Mapping
{
    using System;
    using System.Collections.Generic;
    using Context;

    public class TableMappingsCacheService : ITableMappingsCacheService
    {
        private readonly IDictionary<Type, object> tableMappingCache;

        public TableMappingsCacheService()
        {
            this.tableMappingCache = new Dictionary<Type, object>();
        }

        public ContextTableMappings RetrieveMappings<TContext>() 
            where TContext : IDbContext
        {
            object contextMappings;

            if (this.tableMappingCache.TryGetValue(typeof(TContext), out contextMappings))
            {
                return (ContextTableMappings)contextMappings;
            }

            throw new InvalidOperationException($"No mappings available for {nameof(TContext)} context.");
        }

        public void StoreContextMappings<TContext>(ContextTableMappings mappings) 
            where TContext : IDbContext
        {
            if (mappings == null)
            {
                throw new ArgumentNullException(nameof(mappings));
            }

            this.tableMappingCache.Add(typeof(TContext), mappings);
        }
    }
}
