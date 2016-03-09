using System;
using System.Collections.Generic;
using Amazon.APIGateway;
using Amazon.APIGateway.Model;
using log4net;

namespace Importer.Swagger.Aws.Impl
{
    public class ApiGatewaySdkSwaggerApiImporter : ISwaggerApiImporter
    {
        protected IAmazonAPIGateway Gateway;
        protected ILog Log = LogManager.GetLogger(typeof(ApiGatewaySdkSwaggerApiImporter));
        protected HashSet<string> ProcessedModels = new HashSet<string>();

        private readonly IApiGatewaySdkModelProvider modelProvider;
        private readonly IApiGatewaySdkDeploymentProvider deploymentProvider;
        private readonly IApiGatewaySdkResourceProvider resourceProvider;
        readonly ILog log = LogManager.GetLogger(typeof (ApiGatewaySdkSwaggerApiImporter));

        public ApiGatewaySdkSwaggerApiImporter(IAmazonAPIGateway gateway, IApiGatewaySdkModelProvider modelProvider, IApiGatewaySdkResourceProvider resourceProvider, IApiGatewaySdkDeploymentProvider deploymentProvider)
        {
            Gateway = gateway;
            this.modelProvider = modelProvider;
            this.resourceProvider = resourceProvider;
            this.deploymentProvider = deploymentProvider;
        }

        public string CreateApi(SwaggerDocument swagger, string name)
        {
            ProcessedModels.Clear();

            Log.InfoFormat("Creating API with Name {0}", name);

            var request = new CreateRestApiRequest
            {
                Name = !string.IsNullOrEmpty(swagger.Info.Title) ? swagger.Info.Title : name,
                Description = swagger.Info.Description
            };

            var response = Gateway.CreateRestApi(request);

            try
            {
                var api = response.RestApi();

                var rootResource = this.GetRootResource(api);
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

            Gateway.DeleteRestApi(new DeleteRestApiRequest() {
                RestApiId = apiId
            });
        }

        public string ProvisionApiKey(string apiId, string name, string stage)
        {
            return deploymentProvider.CreateApiKey(apiId, name, stage);
        }

        private Resource GetRootResource(RestApi api)
        {
            foreach (var resource in BuildResourceList(api))
            {
                if ("/".Equals(resource.Path))
                {
                    return resource;
                }
            }

            return null;
        }

        private List<Resource> BuildResourceList(RestApi api)
        {
            var resourceList = new List<Resource>();

            var resources = Gateway.GetResources(new GetResourcesRequest()
            {
                RestApiId = api.Id,
                Limit = 500
            });

            resourceList.AddRange(resources.Items);

            //ToDo:Travese next link
            //while (resources.._isLinkAvailable("next")) {
            //{
            //    resources = resources.getNext();
            //    resourceList.addAll(resources.getItem());
            //}

            return resourceList;
        }
    }
}
    