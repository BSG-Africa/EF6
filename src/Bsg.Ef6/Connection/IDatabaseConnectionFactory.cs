namespace Bsg.Ef6.Connection
{
    using System.Data.Common;

    public interface IDatabaseConnectionFactory
    {
        DbConnection BuildConnectionFromKey(string connectionStringKey);
    }
}