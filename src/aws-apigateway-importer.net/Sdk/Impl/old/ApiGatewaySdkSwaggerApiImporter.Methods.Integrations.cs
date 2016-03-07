using System.Collections.Generic;
using Amazon.APIGateway;
using Amazon.APIGateway.Model;
using Newtonsoft.Json.Linq;

namespace ApiGatewayImporter.Sdk.Impl.old
{
    public partial class ApiGatewaySdkSwaggerApiImporter
    {
        private void CreateIntegration(RestApi api, Resource resource, Method method, IDictionary<string, object> vendorExtensions)
        {
            if (!vendorExtensions.ContainsKey(Constants.EXTENSION_INTEGRATION))
            {
                return;
            }

            var integ = (JObject)vendorExtensions[Constants.EXTENSION_INTEGRATION];
            var type = IntegrationType.FindValue(GetStringValue(integ["type"]).ToUpper());

            Log.InfoFormat("Creating integration with type {0}", type);

            var request = new PutIntegrationRequest()
            {
                RestApiId = api.Id,
                ResourceId = resource.Id,
                Type = type,
                Uri = GetStringValue(integ["uri"]),
                Credentials = GetStringValue(integ["credentials"]),
                HttpMethod = GetStringValue(integ["httpMethod"]),
                IntegrationHttpMethod = GetStringValue(integ["httpMethod"]),
                RequestParameters = integ["requestParameters"]?.ToObject<Dictionary<string, string>>(),
                RequestTemplates =  integ["requestTemplates"]?.ToObject<Dictionary<string, string>>(),
                CacheNamespace = GetStringValue(integ["cacheNamespace"]),
                CacheKeyParameters = integ["cacheKeyParameters"]?.ToObject<List<string>>()
            };

            var integration = Client.PutIntegration(request);
            CreateIntegrationResponses(api, resource, integration, integ.ToDictionary());
        }

        private void CreateIntegrationResponses(RestApi api, Resource resource, PutIntegrationResponse integration, IDictionary<string, object> integ)
        {
            var responses = integ.ToDictionary<string, object>("responses");

            responses?.ForEach(e =>
            {
                var pattern = e.Key.Equals("default") ? null : e.Key;
                var response = e.Value as IDictionary<string, object>;

                if (response != null)
                {
                    var status = response["statusCode"].ToString();
                    var responseParams = response.ToDictionary<string, string>("responseParameters");
                    var responseTemplates = response.ToDictionary<string, string>("responseTemplates");

                    var request = new PutIntegrationResponseRequest()
                    {
                        RestApiId = api.Id,
                        ResourceId = resource.Id,
                        HttpMethod = integration.HttpMethod,
                        ResponseParameters = responseParams,
                        ResponseTemplates = responseTemplates,
                        SelectionPattern = pattern,
                        StatusCode = status
                    };

                    Client.PutIntegrationResponse(request);
                }
            });
        }
    }
}
