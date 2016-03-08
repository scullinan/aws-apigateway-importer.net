using Amazon.APIGateway.Model;
using Importer.Swagger;

namespace Importer.Aws
{
    public interface IApiGatewaySdkResourceProvider
    {
        void CreateResources(RestApi api, Resource rootResource, SwaggerDocument swagger, bool createMethods);
    }
}