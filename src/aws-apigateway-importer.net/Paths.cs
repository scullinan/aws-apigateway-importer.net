namespace Importer
{
    public static class Paths
    {
        public const string CloudwatchRoleArn = "/cloudwatchRoleArn";

        public static class Logging
        {
            public const string MetricsEnabled = "/*/*/metrics/enabled";
            public const string LogLevel = "/*/*/logging/loglevel";
            public const string DataTrace = "/*/*/logging/dataTrace";
        }

        public static class Caching
        {
            public const string Enabled = "/*/*/caching/enabled";
            public const string TtlInSeconds = "/*/*/caching/ttlInSeconds";
            public const string DataEncrypted = "/*/*/caching/dataEncrypted";
        }

        public static class Throttling
        {
            public const string BurstLimit = "/*/*/throttling/burstLimit";
            public const string RateLimit = "/*/*/throttling/rateLimit";
        }

    }
}