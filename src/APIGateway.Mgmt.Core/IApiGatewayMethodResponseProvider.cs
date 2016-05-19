using System.Collections.Generic;
using Amazon.APIGateway.Model;
using APIGateway.Swagger;

namespace APIGateway.Management
{
    public interface IApiGatewayMethodResponseProvider
    {
        void CreateMethodResponses(RestApi api, Resource resource, Method method, SwaggerDocument swagger, string modelContentType, IDictionary<string, Response> responses);
        void UpdateMethodResponses(RestApi api, Resource resource, Method method, SwaggerDocument swagger, string modelContentType, IDictionary<string, Response> responses);
    }
}