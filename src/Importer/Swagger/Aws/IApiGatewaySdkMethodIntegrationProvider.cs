using System.Collections.Generic;
using Amazon.APIGateway.Model;

namespace Importer.Swagger.Aws
{
    public interface IApiGatewaySdkMethodIntegrationProvider
    {
        void CreateIntegration(RestApi api, Resource resource, Method method, Dictionary<string, object> vendorExtensions);
    }
}