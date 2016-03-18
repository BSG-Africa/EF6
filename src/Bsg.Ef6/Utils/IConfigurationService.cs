namespace Bsg.Ef6.Utils
{
    using System.Configuration;

    public interface IConfigurationService
    {
        string RetrieveAppSetting(string appSettingKey);

        ConnectionStringSettings RetrieveConnectionString(string connectionName);
    }
}