using Amazon.APIGateway.Model;

namespace Importer.Swagger.Aws
{
    public interface IApiGatewaySdkResourceProvider
    {
        void CreateResources(RestApi api, Resource rootResource, SwaggerDocument swagger, bool createMethods);
        void UpdateResources(RestApi api, Resource rootResource, SwaggerDocument swagger);
        void CleanupResources(RestApi api, SwaggerDocument swagger);
        void DeleteResources(RestApi api);
    }
}