namespace Bsg.Ef6.Context
{
    using System;
    using System.Data.Common;
    using System.Diagnostics.CodeAnalysis;
    using Timeout;
    using Utils;

    // TODO check performance from many typeOfs
    // as well as the Activator.CreateInstance
    public class DbContextFactory : IDbContextFactory
    {
        private readonly IConfigurationService configService;

        private readonly ITimeoutCacheService timeoutCacheService;

        private readonly IDatabaseConnectionFactory connectionFactory;

        public DbContextFactory(
            IConfigurationService configService,
            ITimeoutCacheService timeoutCacheService,
            IDatabaseConnectionFactory databaseConnectionFactory)
        {
            this.connectionFactory = databaseConnectionFactory;
            this.timeoutCacheService = timeoutCacheService;
            this.configService = configService;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public IDbContext BuildContext<TContext>()
            where TContext : IDbContext
        {
            // connection will be closed by the context
            var connection = this.BuildConnection<TContext>();
            var context = this.BuildContext<TContext>(connection, true);
            context.SetCommandTimeout(this.timeoutCacheService.ContextTimeout<TContext>());
            this.AttachLogger<TContext>(context);

            return context;
        }

        public void BindViews<TContext>()
            where TContext : IDbContext
        {
            // connection will be disposed manually
            using (var connection = this.BuildConnection<TContext>())
            {
                using (var context = this.BuildContext<TContext>(connection, false))
                {
                    this.AttachLogger<TContext>(context);
                    context.BindPreGeneratedViews();
                }
            }
        }

        private void AttachLogger<TContext>(IDbContext context)
        {
            if (this.configService.RetrieveAppSetting($"{typeof(TContext).Name}.EnableDbContextConsoleLogging").ToUpperInvariant() == "TRUE")
            {
                context.SetLogger(Console.Write);
            }
        }

        private TContext BuildContext<TContext>(DbConnection dbConnection, bool contextOwnsConnection)
        {
            return (TContext)Activator.CreateInstance(typeof(TContext), dbConnection, contextOwnsConnection);
        }

        private DbConnection BuildConnection<TContext>()
        {
            return this.connectionFactory.BuildConnectionFromKey($"{typeof(TContext).Name}");
        }
    }
}