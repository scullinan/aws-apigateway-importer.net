namespace AWS.APIGateway
{
    public class DeploymentConfig
    {
        public string Description { get; set; }
        public string StageName { get; set; }
        public string StageDescription { get; set; }
        public LoggingConfig Logging { get; set; }
        public CachingConfig Caching { get; set; }
        public ThrottlingConfig Throttling{ get; set; }

        public class LoggingConfig
        {
            public bool Enabled { get; set; }
            public string CloudwatchRoleArn { get; set; }
            public bool MetricsEnabled { get; set; }
            public string LoggingLevel { get; set; }
            public bool DataTraceEnabled { get; set; }
        }

        public class CachingConfig
        {
            public bool Enabled { get; set; }
            public string ClusterSize { get; set; }
            public int CacheTtlInSeconds { get; set; }
            public int CacheDataEncrypted { get; set; }
        }

        public class ThrottlingConfig
        {
            public int RateLimit { get; set; }
            public int BurstLimit { get; set; }
        }
    }
}