namespace APIGateway.Management
{
    public interface IApiGatewayProviderFactory
    {
        IApiGatewayProvider Create();
    }
}