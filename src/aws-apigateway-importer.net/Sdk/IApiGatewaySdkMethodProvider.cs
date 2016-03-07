using System.Collections.Generic;
using Amazon.APIGateway.Model;

namespace ApiGatewayImporter.Sdk
{
    public interface IApiGatewaySdkMethodProvider
    {
        void CreateMethods(RestApi api, SwaggerDocument swagger, Resource resource, PathItem path, IList<string> apiProduces);
    }
}