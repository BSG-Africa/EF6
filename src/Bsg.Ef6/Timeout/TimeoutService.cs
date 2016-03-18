namespace Bsg.Ef6.Timeout
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Context;

    public class TimeoutService : ITimeoutService
    {
        private readonly ITimeoutCacheService timeoutCacheService;
        private readonly ITimeoutFactory timeoutFactory;

        public TimeoutService(
            ITimeoutCacheService timeoutCacheService,
            ITimeoutFactory timeoutFactory)
        {
            this.timeoutFactory = timeoutFactory;
            this.timeoutCacheService = timeoutCacheService;
        }

        public void BuildAndCacheAllTimeouts(Assembly assemblyWithContexts)
        {
            if (assemblyWithContexts == null)
            {
                throw new ArgumentNullException(nameof(assemblyWithContexts));
            }

            var dbContextInterfaceType = typeof(IDbContext);

            var contextTypes = assemblyWithContexts.GetTypes()
                .Where(t => !t.IsAbstract && dbContextInterfaceType.IsAssignableFrom(t))
                .ToList();

            foreach (var contextType in contextTypes)
            {
                var genericBuildTimeoutsMethod = this.GetGenericMethodFromFunc(() => this.timeoutFactory.BuildTimeouts<IDbContext>(), contextType);

                var timeout = (ContextTimeouts)genericBuildTimeoutsMethod.Invoke(this.timeoutFactory, null);

                var genericStoreTimeoutsMethod =
                    this.GetGenericMethodFromAction(e => this.timeoutCacheService.StoreTimeouts<IDbContext>(timeout), contextType);

                genericStoreTimeoutsMethod.Invoke(this.timeoutCacheService, new object[] { timeout });
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
