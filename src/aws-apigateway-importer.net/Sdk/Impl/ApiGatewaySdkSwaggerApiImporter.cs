using System;
using Amazon.APIGateway.Model;
using log4net;

namespace ApiGatewayImporter.Sdk.Impl
{
    public class ApiGatewaySdkSwaggerApiImporter : ApiGatewaySdkApiImporter, ISwaggerApiImporter
    {
        private readonly IApiGatewaySdkModelProvider modelProvider;
        private readonly IApiGatewaySdkDeploymentProvider deploymentProvider;
        private readonly IApiGatewaySdkResourceProvider resourceProvider;
        readonly ILog log = LogManager.GetLogger(typeof (ApiGatewaySwaggerApiFileImporter));


        public ApiGatewaySdkSwaggerApiImporter()
        {
            this.modelProvider = new ApiGatewaySdkModelProvider(ProcessedModels, Client);

            this.resourceProvider = new ApiGatewaySdkResourceProvider(Client,
                new ApiGatewaySdkMethodProvider(ProcessedModels,
                    Client,
                    modelProvider,
                    new ApiGatewaySdkMethodResponseProvider(ProcessedModels, Client, modelProvider),
                    new ApiGatewaySdkMethodParameterProvider(Client),
                    new ApiGatewaySdkMethodIntegrationProvider(Client)));

            this.deploymentProvider = new ApiGatewaySdkDeploymentProvider(Client);
        }

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
                modelProvider.DeleteDefaultModels(api);
                modelProvider.CreateModels(api, swagger.Definitions, swagger.Produces);
                resourceProvider.CreateResources(api, rootResource, swagger, true);

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
            //Todo
        }

        public void Deploy(string apiId, DeploymentConfig config)
        {
            log.InfoFormat("Deploying API {0}", apiId);

            deploymentProvider.CreateDeployment(apiId, config);
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
            return deploymentProvider.CreateApiKey(apiId, name, stage);
        }

    }
}
    