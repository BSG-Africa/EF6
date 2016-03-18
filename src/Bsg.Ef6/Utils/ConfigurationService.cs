namespace Bsg.Ef6.Utils
{
    using System.Configuration;

    public class ConfigurationService : IConfigurationService
    {
        public string RetrieveAppSetting(string appSettingKey)
        {
            var value = ConfigurationManager.AppSettings[appSettingKey];

            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            return value;
        }

        public ConnectionStringSettings RetrieveConnectionString(string connectionName)
        {
            return ConfigurationManager.ConnectionStrings[connectionName];
        }
    }
}
