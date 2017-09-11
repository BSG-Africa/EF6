namespace Bsg.Ef6.Connection
{
    using System.Data.Common;

    public interface IMsSqlConnectionFactory
    {
        DbConnection BuildConnection(string connection);
    }
}
