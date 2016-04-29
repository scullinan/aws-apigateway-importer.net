namespace APIGateway.Management
{
    public interface IApiGatewayProviderFactory
    {
        IApiGatewayProvider Create(string version = "v2.0");
    }
}