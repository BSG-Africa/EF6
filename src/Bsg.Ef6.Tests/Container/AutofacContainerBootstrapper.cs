namespace Bsg.Ef6.Tests.Container
{
    using System;
    using System.Reflection;
    using Autofac;

    public abstract class AutofacContainerBootstrapper
    {
        public IServiceProvider BuildAutofacContainer()
        {
            var builder = new ContainerBuilder();
            this.ConfigureContainerBeforeBuild(builder);
            var container = (IServiceProvider)builder.Build();
            this.ConfigureContainerAfterBuild(container);
            return container;
        }

        public abstract void ConfigureContainerBeforeBuild(ContainerBuilder builder);

        public abstract void ConfigureContainerAfterBuild(IServiceProvider container);

        protected void RegisterDefaultConventionTypes(ContainerBuilder builder, params Assembly[] assembliesToCheck)
        {
            // Register all Repositories using convention
            builder.RegisterAssemblyTypes(assembliesToCheck)
                .Where(t => t.Name.EndsWith("Repository", StringComparison.OrdinalIgnoreCase))
                .AsImplementedInterfaces();

            // Register all Services using convention
            builder.RegisterAssemblyTypes(assembliesToCheck)
                .Where(t => t.Name.EndsWith("Service", StringComparison.OrdinalIgnoreCase))
                .AsImplementedInterfaces();

            // Register all Factories using convention
            builder.RegisterAssemblyTypes(assembliesToCheck)
                .Where(t => t.Name.EndsWith("Factory", StringComparison.OrdinalIgnoreCase))
                .AsImplementedInterfaces();
        }
    }
}
