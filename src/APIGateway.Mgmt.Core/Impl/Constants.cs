namespace APIGateway.Management.Impl
{
    public static class Constants
    {
        public const string DefaultProducesContentType = "application/json";
        public const string ExtensionAuth = "x-amazon-apigateway-auth";
        public const string ExtensionIntegration = "x-amazon-apigateway-integration";
    }

    public static class ListCommands
    {
        public const string Apis = "apis";
        public const string Keys = "keys";
        public const string Ops = "ops";
    }

    public static class ApiKeyCommands
    {
        public const string Create = "create";
        public const string Delete = "delete";
    }

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

    public static class Operations
    {
        public const string Add = "add";
        public const string Remove = "remove";
        public const string Replace = "replace";
        public const string Move = "move";
        public const string Copy = "copy";
        public const string Test = "test";
    }

}