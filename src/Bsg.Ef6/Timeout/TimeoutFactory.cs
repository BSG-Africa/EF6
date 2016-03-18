namespace Bsg.Ef6.Timeout
{
    using System.Globalization;
    using Context;
    using Utils;

    public class TimeoutFactory : ITimeoutFactory
    {
        private readonly IConfigurationService configurationService;

        public TimeoutFactory(IConfigurationService configurationService)
        {
            this.configurationService = configurationService;
        }

        public ContextTimeouts BuildTimeouts<TContext>()
            where TContext : IDbContext
        {
            var context = typeof(TContext).Name;

            return new ContextTimeouts(
                this.GetTimeout($"{context}.ContextTimeout", 300),
                this.GetTimeout($"{context}.BulkInsertTimeout", 300),
                this.GetTimeout($"{context}.BulkUpdateTimeout", 600));
        }

        private int GetTimeout(string key, int defaultTimeout)
        {
            var timeoutSetting = this.configurationService.RetrieveAppSetting(key);
            var timeoutValue = this.ParseTimeout(timeoutSetting);
            return timeoutValue ?? defaultTimeout;
        }

        private int? ParseTimeout(string value)
        {
            int timeout;

            if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out timeout))
            {
                return timeout;
            }

            return null;
        }
    }
}
