namespace Bsg.Ef6.Tests.Container
{
    using System;
    using System.Reflection;
    using Autofac;
    using Context;
    using Data.Repo;
    using Extensions;
    using Mapping;
    using Repo;
    using Timeout;

    public class TestIocBootstrapper : AutofacContainerBootstrapper
    {
        public override void ConfigureContainerBeforeBuild(ContainerBuilder builder)
        {
            var ef6Assembly = Assembly.GetAssembly(typeof(IDbContext));
            var testAssembly = Assembly.GetAssembly(typeof(TestIocBootstrapper));

            // Setup Scoped Container factory lamda expression
            builder.Register(ctx => ctx.Resolve<ILifetimeScope>().BeginLifetimeScope() as IServiceProvider).As<IServiceProvider>();

            this.RegisterDefaultConventionTypes(builder, ef6Assembly, testAssembly);

            // register generic repository as open generic 
            builder.RegisterGeneric(typeof(GenericRepository<,>)).As(typeof(IGenericRepository<,>));
            builder.RegisterGeneric(typeof(PrimaryRepository<>)).As(typeof(IPrimaryRepository<>));

            // register individual singleton per scope services 
            // or other services which don't fit default conventions
            // will override any previous registrations
            builder.RegisterGeneric(typeof(DbContextSession<>)).As(typeof(IDbContextSession<>)).InstancePerLifetimeScope();
            
            // Singleton instances across the entire application
            builder.RegisterType<TableMappingsCacheService>().As<ITableMappingsCacheService>().SingleInstance();
            builder.RegisterType<TimeoutCacheService>().As<ITimeoutCacheService>().SingleInstance();
        }

        public override void ConfigureContainerAfterBuild(IServiceProvider container)
        {
            var testAssembly = Assembly.GetAssembly(typeof(TestIocBootstrapper));

            // Pre Gen EF Views 
            var dbContextFactory = container.GetService<IContextService>();
            dbContextFactory.PreGenerateAllContextViews(testAssembly);

            // Cache Timeouts
            var timeoutService = container.GetService<ITimeoutService>();
            timeoutService.BuildAndCacheAllTimeouts(testAssembly);

            // Cache Mappings
            var mappingService = container.GetService<ITableMappingService>();
            mappingService.BuildAndCacheAllTableMappings(testAssembly);
        }
    }
}