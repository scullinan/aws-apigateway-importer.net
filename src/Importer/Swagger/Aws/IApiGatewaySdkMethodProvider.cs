using System.Collections.Generic;
using Amazon.APIGateway.Model;

namespace Importer.Swagger.Aws
{
    public interface IApiGatewaySdkMethodProvider
    {
        void CreateMethods(RestApi api, SwaggerDocument swagger, Resource resource, PathItem path, IList<string> apiProduces);
        void UpdateMethods(RestApi api, SwaggerDocument swagger);
    }
}