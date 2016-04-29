using Amazon.APIGateway.Model;
using APIGateway.Swagger;

namespace APIGateway.Management
{
    public interface IApiGatewayResourceProvider
    {
        void CreateResources(RestApi api, Resource rootResource, SwaggerDocument swagger, bool createMethods);
        void UpdateResources(RestApi api, Resource rootResource, SwaggerDocument swagger);
        void CleanupResources(RestApi api, SwaggerDocument swagger);
        void DeleteResources(RestApi api);
    }
}