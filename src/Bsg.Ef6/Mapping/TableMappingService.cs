namespace Bsg.Ef6.Mapping
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Context;
    using Timeout;

    public class TableMappingService : ITableMappingService
    {
        private readonly ITableMappingsCacheService tableMappingsCacheService;
        private readonly ITableMappingsFactory tableMappingsFactory;

        public TableMappingService(
            ITableMappingsCacheService tableMappingsCacheService,
            ITableMappingsFactory tableMappingsFactory)
        {
            this.tableMappingsFactory = tableMappingsFactory;
            this.tableMappingsCacheService = tableMappingsCacheService;
        }

        public void BuildAndCacheAllTableMappings(Assembly assemblyWithContexts)
        {
            if (assemblyWithContexts == null)
            {
                throw new ArgumentNullException(nameof(assemblyWithContexts));
            }

            var dbContextInterfaceType = typeof(IDbContext);

            var contextTypes = assemblyWithContexts.GetTypes()
                .Where(t => !t.IsAbstract && dbContextInterfaceType.IsAssignableFrom(t))
                .ToList();

            this.BuildAndCacheAllTableMappings(contextTypes);
        }

        public void BuildAndCacheAllTableMappings(params IDbContext[] contexts)
        {
            if (contexts == null)
            {
                throw new ArgumentNullException(nameof(contexts));
            }

            var contextTypes = contexts.Select(c => c.GetType())
                .Where(t => !t.IsAbstract)
                .ToList();

            this.BuildAndCacheAllTableMappings(contextTypes);
        }

        private void BuildAndCacheAllTableMappings(IList<Type> contextTypes)
        {
            foreach (var contextType in contextTypes)
            {
                var genericBuildTimeoutsMethod = this.GetGenericMethodFromFunc(() => this.tableMappingsFactory.BuildTableMappings<IDbContext>(), contextType);
                var mappings = (ContextTableMappings)genericBuildTimeoutsMethod.Invoke(this.tableMappingsFactory, null);

                var genericStoreTimeoutsMethod =
                    this.GetGenericMethodFromAction(e => this.tableMappingsCacheService.StoreContextMappings<IDbContext>(mappings), contextType);

                genericStoreTimeoutsMethod.Invoke(this.tableMappingsCacheService, new object[] { mappings });
            }
        }

        private MethodInfo GetGenericMethodFromFunc(Expression<Func<object>> expression, Type contextType)
        {
            return ((MethodCallExpression)expression.Body)
                .Method
                .GetGenericMethodDefinition()
                .MakeGenericMethod(contextType);
        }

        private MethodInfo GetGenericMethodFromAction(Expression<Action<object>> expression, Type contextType)
        {
            return ((MethodCallExpression)expression.Body)
                .Method
                .GetGenericMethodDefinition()
                .MakeGenericMethod(contextType);
        }
    }
}
