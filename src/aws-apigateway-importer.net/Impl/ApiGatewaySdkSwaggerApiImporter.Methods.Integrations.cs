using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.APIGateway;
using Amazon.APIGateway.Model;

namespace aws_apigateway_importer.net.Impl
{
    public partial class ApiGatewaySdkSwaggerApiImporter
    {
        private async Task CreateIntegration(RestApi api, Resource resource, Method method, IDictionary<string, object> vendorExtensions)
        {
            if (!vendorExtensions.ContainsKey(EXTENSION_INTEGRATION))
            {
                return;
            }

            var integ = (IDictionary <string, Dictionary<string, string>> )vendorExtensions[EXTENSION_INTEGRATION];
			var type = IntegrationType.FindValue(integ["type"].ToString().ToUpper()); 

            Log.InfoFormat("Creating integration with type {0}", type);

            var request = new PutIntegrationRequest()
            {
                RestApiId = api.Id,
                ResourceId = resource.Id,
                Type = type,
                Uri = GetStringValue(integ["uri"]),
                Credentials = GetStringValue(integ["credentials"]),
                HttpMethod = GetStringValue(integ["httpMethod"]),
                RequestParameters = integ["requestParameters"],
                RequestTemplates = integ["requestParameters"],
                CacheNamespace = GetStringValue(integ["cacheNamespace"]),
                CacheKeyParameters = integ["cacheKeyParameters"].Select(x => x.Value).ToList()
            };

            var integration = await Client.PutIntegrationAsync(request);

            await CreateIntegrationResponses(integration, integ);
        }

        private async Task CreateIntegrationResponses(PutIntegrationResponse integration, IDictionary<string, Dictionary<string, string>> integ)
        {
            // todo: avoid unchecked casts
			var responses = (IDictionary<string, Dictionary<string, object>>)integ["responses"];

            responses.ForEach(async e => {
                var pattern = e.Key.Equals("default") ? null : e.Key;
                var response = e.Value;

                var status = response["statusCode"].ToString();

                var request = new PutIntegrationResponseRequest()
                {
                    ResponseParameters = (Dictionary<string, string>)response["responseParameters"],
					ResponseTemplates = (Dictionary<string, string>)response["responseTemplates"],
					SelectionPattern = pattern,
					StatusCode = status

                };

                await Client.PutIntegrationResponseAsync(request);
            });
        }
    }
}
