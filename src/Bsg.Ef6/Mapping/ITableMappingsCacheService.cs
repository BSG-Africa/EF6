namespace Bsg.Ef6.Mapping
{
    using Context;

    public interface ITableMappingsCacheService
    {
        ContextTableMappings RetrieveMappings<TContext>()
            where TContext : IDbContext;

        void StoreContextMappings<TContext>(ContextTableMappings mappings)
            where TContext : IDbContext;
    }
}