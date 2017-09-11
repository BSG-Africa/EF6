namespace Bsg.Ef6.Connection
{
    using System.Data.Common;
    using Utils;

    public class DatabaseConnectionFactory : IDatabaseConnectionFactory
    {
        private readonly IConfigurationService configService;
        private readonly IMsSqlConnectionFactory msSqlConnectionService;
        private readonly INonMsSqlConnectionFactory nonMsSqlConnectionFactory;

        public DatabaseConnectionFactory(
            IConfigurationService configService,
            IMsSqlConnectionFactory msSqlConnectionService,
            INonMsSqlConnectionFactory nonMsSqlConnectionFactory)
        {
            this.nonMsSqlConnectionFactory = nonMsSqlConnectionFactory;
            this.msSqlConnectionService = msSqlConnectionService;
            this.configService = configService;
        }

        public DbConnection BuildConnectionFromKey(string connectionStringKey)
        {
            var connectionSettings = this.configService.RetrieveConnectionSettings(connectionStringKey);
            var providerName = connectionSettings.ProviderName.ToLowerInvariant();

            switch (providerName)
            {
                case "system.data.sqlclient":
                    return this.msSqlConnectionService.BuildConnection(connectionSettings.ConnectionString);
                default:
                    return this.nonMsSqlConnectionFactory.BuildConnection(connectionSettings.ConnectionString, providerName);
            }
        }
    }
}