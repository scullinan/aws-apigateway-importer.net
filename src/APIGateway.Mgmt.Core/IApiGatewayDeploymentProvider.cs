namespace APIGateway.Management
{
    public interface IApiGatewayDeploymentProvider
    {
        void CreateDeployment(string apiId, DeploymentDocument config);
        void CreateDomain(DeploymentDocument config);
        //ToDo:Move
        string CreateApiKey(string apiId, string name, string stage);
        void DeleteApiKey(string key);
        void ClearApiKeys();
    }
}