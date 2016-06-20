namespace Bsg.Ef6.Mapping
{
    using System.Reflection;
    using Context;

    public interface ITableMappingService
    {
        void BuildAndCacheAllTableMappings(Assembly assemblyWithContexts);

        void BuildAndCacheAllTableMappings(params IDbContext[] contexts);
    }
}