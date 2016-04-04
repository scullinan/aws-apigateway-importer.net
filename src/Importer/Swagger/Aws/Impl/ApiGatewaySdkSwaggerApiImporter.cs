using System;
using System.Collections.Generic;
using Amazon.APIGateway;
using Amazon.APIGateway.Model;
using log4net;

namespace Importer.Swagger.Aws.Impl
{
    public class ApiGatewaySdkSwaggerApiImporter : ISwaggerApiImporter
    {
        protected IAmazonAPIGateway gateway;
        protected ILog Log = LogManager.GetLogger(typeof(ApiGatewaySdkSwaggerApiImporter));
        
        private readonly IApiGatewaySdkModelProvider modelProvider;
        private readonly IApiGatewaySdkDeploymentProvider deploymentProvider;
        private readonly IApiGatewaySdkResourceProvider resourceProvider;
        private readonly IApiGatewaySdkMethodProvider methodProvider;

        public ApiGatewaySdkSwaggerApiImporter(IAmazonAPIGateway gateway, IApiGatewaySdkModelProvider modelProvider, IApiGatewaySdkResourceProvider resourceProvider, IApiGatewaySdkMethodProvider methodProvider, IApiGatewaySdkDeploymentProvider deploymentProvider)
        {
            this.gateway = gateway;
            this.modelProvider = modelProvider;
            this.resourceProvider = resourceProvider;
            this.methodProvider = methodProvider;
            this.deploymentProvider = deploymentProvider;
        }

        public string CreateApi(SwaggerDocument swagger, string name)
        {
            Log.InfoFormat("Creating API with Name {0}", name);

            var request = new CreateRestApiRequest
            {
                Name = !string.IsNullOrEmpty(swagger.Info.Title) ? swagger.Info.Title : name,
                Description = swagger.Info.Description
            };

            var response = gateway.WaitAndRetry(x => x.CreateRestApi(request));

            try
            {
                var api = response.RestApi();

                var rootResource = this.GetRootResource(api);
                modelProvider.DeleteDefaultModels(api);
                modelProvider.CreateModels(api, swagger);

                resourceProvider.CreateResources(api, rootResource, swagger, true);

                return api.Id;
            }
            catch (Exception ex)
            {
                Log.Error("Error creating API, rolling back", ex);
            }

            return string.Empty;
        }

        public void UpdateApi(string apiId, SwaggerDocument swagger)
        {
            Log.InfoFormat("Updating API {0}", apiId);

            var api = GetApi(apiId);
            var rootResource = GetRootResource(api);

            modelProvider.UpdateModels(api, swagger);
            resourceProvider.UpdateResources(api, rootResource, swagger);
            methodProvider.UpdateMethods(api, swagger);

            methodProvider.CleanupMethods(api, swagger);
            resourceProvider.CleanupResources(api, swagger);
            modelProvider.CleanupModels(api);
        }

        public void PatchApi(string apiId, SwaggerDocument swagger)
        {
            Log.InfoFormat("Patching API {0}", apiId);

            var response = gateway.WaitAndRetry(x => x.GetRestApi(new GetRestApiRequest() { RestApiId = apiId }));

            try
            {
                var api = response.RestApi();
                var rootResource = this.GetRootResource(api);

                modelProvider.UpdateModels(api, swagger);
                resourceProvider.UpdateResources(api, rootResource, swagger);
                methodProvider.UpdateMethods(api, swagger);
            }
            catch (Exception ex)
            {
                Log.Error("Error creating API, rolling back", ex);
            }
        }

        public void Deploy(string apiId, DeploymentConfig config)
        {
            Log.InfoFormat("Deploying API {0}", apiId);

            deploymentProvider.CreateDeployment(apiId, config);
        }

        public void DeleteApi(string apiId)
        {
            Log.InfoFormat("Deleting API {0}", apiId);

            gateway.WaitAndRetry(x => x.DeleteRestApi(new DeleteRestApiRequest() {
                RestApiId = apiId
            }));
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

            var resources = gateway.WaitAndRetry(x => x.GetResources(new GetResourcesRequest()
            {
                RestApiId = api.Id,
                Limit = 500
            }));

            resourceList.AddRange(resources.Items);

            //ToDo:Travese next link
            //while (resources.._isLinkAvailable("next")) {
            //{
            //    resources = resources.getNext();
            //    resourceList.addAll(resources.getItem());
            //}

            return resourceList;
        }

        private RestApi GetApi(string apiId)
        {
            var api = gateway.WaitAndRetry(x => x.GetRestApi(new GetRestApiRequest() {RestApiId = apiId}));
            return new RestApi() { Id = api.Id, Name = api.Name };
        }
    }
}
    