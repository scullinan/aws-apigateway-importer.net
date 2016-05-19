using System.Collections.Generic;
using Amazon.APIGateway.Model;

namespace APIGateway.Management
{
    public interface IApiGatewayMethodIntegrationProvider
    {
        void CreateIntegration(RestApi api, Resource resource, Method method, IDictionary<string, object> vendorExtensions);
    }
}