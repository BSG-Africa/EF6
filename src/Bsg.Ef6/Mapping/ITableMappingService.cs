namespace Bsg.Ef6.Mapping
{
    using System.Reflection;

    public interface ITableMappingService
    {
        void BuildAndCacheAllTableMappings(Assembly assemblyWithContexts);
    }
}