namespace ApiGatewayImporter.Sdk
{
    public interface IApiGatewaySdkDeploymentProvider
    {
        void CreateDeployment(string apiId, DeploymentConfig config);
        void CreateDomain(DeploymentConfig config);
        string CreateApiKey(string apiId, string name, string stage);
    }
}