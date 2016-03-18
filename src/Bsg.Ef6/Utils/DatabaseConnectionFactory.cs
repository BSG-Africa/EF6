namespace Bsg.Ef6.Utils
{
    using System.Data.SqlClient;

    public class DatabaseConnectionFactory : IDatabaseConnectionFactory
    {
        private readonly IConfigurationService configService;

        public DatabaseConnectionFactory(IConfigurationService configService)
        {
            this.configService = configService;
        }

        public SqlConnection BuildConnectionFromKey(string connectionStringKey)
        {
            return new SqlConnection(this.ConnectionString(connectionStringKey));
        }

        public SqlConnection BuildConnectionFromString(string connectionString)
        {
            return new SqlConnection(connectionString);
        }

        public string ConnectionString(string connectionName)
        {
            return this.configService.RetrieveConnectionString(connectionName).ConnectionString;
        }
    }
}