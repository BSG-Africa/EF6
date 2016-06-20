namespace Bsg.Ef6.Mapping
{
    using System;
    using System.Reflection;

    public interface ITableMappingService
    {
        void BuildAndCacheAllTableMappings(Assembly assemblyWithContexts);

        void BuildAndCacheAllTableMappings(params Type[] types);
    }
}