 using System.Collections.Generic;
 using Amazon.APIGateway.Model;

namespace ApiGatewayImporter.Sdk
{
    public interface IApiGatewaySdkResourceProvider
    {
        void CreateResources(RestApi api, Resource rootResource, SwaggerDocument swagger, bool createMethods);
    }
}