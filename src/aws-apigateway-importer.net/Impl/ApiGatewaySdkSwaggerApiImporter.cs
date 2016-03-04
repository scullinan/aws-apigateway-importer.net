using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Amazon.APIGateway.Model;
using log4net;
using Newtonsoft.Json;

namespace AWS.APIGateway.Impl
{

    public partial class ApiGatewaySdkSwaggerApiImporter : ApiGatewaySdkApiImporter, ISwaggerApiImporter
    {
        ILog log = LogManager.GetLogger(typeof (ApiGatewaySwaggerApiFileImporter));

        private static string DEFAULT_PRODUCES_CONTENT_TYPE = "application/json";
        private static string EXTENSION_AUTH = "x-amazon-apigateway-auth";
        private static string EXTENSION_INTEGRATION = "x-amazon-apigateway-integration";
        protected SwaggerDocument Swagger;

        public string CreateApi(SwaggerDocument swagger, string name)
        {
            this.Swagger = swagger;
            ProcessedModels.Clear();
            var response = CreateApi(GetApiName(swagger, name), swagger.Info.Description);
            log.InfoFormat("Creating API {0}", response.Id);

            try
            {
                var api = response.RestApi();

                var rootResource = GetRootResource(api);
                DeleteDefaultModels(api);
                CreateModels(api, swagger.Definitions, swagger.Produces);
                CreateResources(api, rootResource, swagger.BasePath, swagger.Produces, swagger.Paths, true);

                return api.Id;

            }
            catch (Exception ex)
            {
                log.Error("Error creating API, rolling back", ex);
            }

            return string.Empty;
        }

        public void UpdateApi(string apiId, SwaggerDocument swagger)
        {
            log.InfoFormat("Updating API {0}", apiId);
        }

        public void Deploy(string apiId, DeploymentConfig config)
        {
            log.InfoFormat("Deploying API {0}", apiId);

            CreateDeployment(apiId, config);
        }

        public void DeleteApi(string apiId)
        {
            log.InfoFormat("Deleting API {0}", apiId);

            Client.DeleteRestApi(new DeleteRestApiRequest()
            {
                RestApiId = apiId
            });
        }

        public string ProvisionApiKey(string apiId, string name, string stage)
        {
            var result = Client.CreateApiKey(new CreateApiKeyRequest()
            {
                Enabled = true,
                Name = name,
                StageKeys = new List<StageKey>()
                {
                    new StageKey() {RestApiId = apiId, StageName = stage}
                }
            });

            return result.Id;
        } 
    }
}
    