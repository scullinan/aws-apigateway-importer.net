using System.Collections.Generic;
using Amazon.APIGateway.Model;

namespace Importer.Swagger.Aws
{
    public interface IApiGatewaySdkMethodResponseProvider
    {
        void CreateMethodResponses(RestApi api, Resource resource, Method method, SwaggerDocument swagger, string modelContentType, IDictionary<string, Response> responses);
        void UpdateMethodResponses(RestApi api, Resource resource, Method method, SwaggerDocument swagger, string modelContentType, IDictionary<string, Response> responses);
    }
}