using System.Collections.Generic;
using Amazon.APIGateway.Model;
using APIGateway.Swagger;

namespace APIGateway.Management
{
    public interface IApiGatewayMethodProvider
    {
        void CreateMethods(RestApi api, SwaggerDocument swagger, Resource resource, PathItem path, IList<string> apiProduces);
        void UpdateMethods(RestApi api, SwaggerDocument swagger);
        void CleanupMethods(RestApi api, SwaggerDocument swagger);
    }
}