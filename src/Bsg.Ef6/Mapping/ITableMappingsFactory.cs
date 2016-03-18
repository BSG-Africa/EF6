namespace Bsg.Ef6.Mapping
{
    using Context;

    public interface ITableMappingsFactory
    {
        ContextTableMappings BuildTableMappings<TContext>()
            where TContext : IDbContext;
    }
}