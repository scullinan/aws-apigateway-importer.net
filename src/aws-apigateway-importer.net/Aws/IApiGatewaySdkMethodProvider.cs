using System.Collections.Generic;
using Amazon.APIGateway.Model;
using Importer.Swagger;

namespace Importer.Aws
{
    public interface IApiGatewaySdkMethodProvider
    {
        void CreateMethods(RestApi api, SwaggerDocument swagger, Resource resource, PathItem path, IList<string> apiProduces);
    }
}