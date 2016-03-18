namespace Bsg.Ef6.Utils
{
    using System.Data.SqlClient;

    public interface IDatabaseConnectionFactory
    {
        SqlConnection BuildConnectionFromKey(string connectionStringKey);

        SqlConnection BuildConnectionFromString(string connectionString);

        string ConnectionString(string connectionName);
    }
}