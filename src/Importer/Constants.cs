namespace Importer
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
    }

    public static class ApiKeyCommands
    {
        public const string Create = "create";
        public const string Delete = "delete";
    }
}