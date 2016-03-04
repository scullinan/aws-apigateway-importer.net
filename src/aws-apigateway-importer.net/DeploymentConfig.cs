using Newtonsoft.Json;

namespace AWS.APIGateway
{
    public class DeploymentConfig
    {
        public string Description { get; set; }

        [JsonRequired]
        public string StageName { get; set; }

        public string StageDescription { get; set; }
        public LoggingConfig Logging { get; set; }
        public CachingConfig Caching { get; set; }
        public DomainConfig Domain { get; set; }

        [JsonRequired]
        public ThrottlingConfig Throttling { get; set; }

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

        public class DomainConfig
        {
            [JsonRequired]
            public string DomainName { get; set; }
            [JsonRequired]
            public string CetificateName { get; set; }
            [JsonRequired]
            public string CetificateBody { get; set; }
            [JsonRequired]
            public string CetificatePrivateKey { get; set; }
            public string CetificateChain { get; set; }
        }
    }
}