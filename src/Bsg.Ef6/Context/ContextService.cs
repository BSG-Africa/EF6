namespace Bsg.Ef6.Context
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    public class ContextService : IContextService
    {
        private readonly IDbContextFactory dbContextFactory;

        public ContextService(IDbContextFactory dbContextFactory)
        {
            this.dbContextFactory = dbContextFactory;
        }

        public void PreGenerateAllContextViews(Assembly assemblyWithContexts)
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
                var genericBuildTimeoutsMethod = this.GetGenericMethodFromAction(() => this.dbContextFactory.BindViews<IDbContext>(), contextType);

                genericBuildTimeoutsMethod.Invoke(this.dbContextFactory, null);
            }
        }

        private MethodInfo GetGenericMethodFromAction(Expression<Action> expression, Type contextType)
        {
            return ((MethodCallExpression)expression.Body)
                .Method
                .GetGenericMethodDefinition()
                .MakeGenericMethod(contextType);
        }
    }
}
