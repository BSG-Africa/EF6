namespace Bsg.Ef6.Timeout
{
    public class ContextTimeouts
    {
        public ContextTimeouts(
            int contextTimeoutSeconds, 
            int bulkInsertTimeoutSeconds,
            int bulkUpdateTimeoutSeconds)
        {
            this.ContextTimeout = contextTimeoutSeconds;
            this.BulkInsertTimeout = bulkInsertTimeoutSeconds;
            this.BulkUpdateTimeout = bulkUpdateTimeoutSeconds;
        }

        public int ContextTimeout { get; }

        public int BulkInsertTimeout { get; }

        public int BulkUpdateTimeout { get; }
    }
}
