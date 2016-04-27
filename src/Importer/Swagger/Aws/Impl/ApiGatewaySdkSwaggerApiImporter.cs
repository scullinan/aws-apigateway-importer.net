using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
                modelProvider.DeleteModels(api);
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

        public void MergeApi(string apiId, SwaggerDocument swagger)
        {
            Log.InfoFormat("Merging API {0}", apiId);

            var response = gateway.WaitAndRetry(x => x.GetRestApi(new GetRestApiRequest() { RestApiId = apiId }));

            var api = response.RestApi();
            var rootResource = this.GetRootResource(api);

            modelProvider.UpdateModels(api, swagger);
            resourceProvider.UpdateResources(api, rootResource, swagger);
            methodProvider.UpdateMethods(api, swagger);
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

        public void DeleteApiKey(string key)
        {
            deploymentProvider.DeleteApiKey(key);
        }

        public void WipeApi(string apiId)
        {
            Log.InfoFormat("Wiping API {0}", apiId);

            var response = gateway.WaitAndRetry(x => x.GetRestApi(new GetRestApiRequest() { RestApiId = apiId }));
            var api = response.RestApi();

            resourceProvider.DeleteResources(api);
            modelProvider.DeleteModels(api);
        }

        public IDictionary<string, string> ListApis()
        {
            var results = gateway.PageWaitAndRetry<RestApi>((x, limit, pos) => x.GetRestApis(new GetRestApisRequest() {
                Limit = limit,
                Position = pos
            }));

            return results.ToDictionary(x => x.Id, x => x.Name);
        }

        public IDictionary<string, Key> ListKeys()
        {
            var results = gateway.PageWaitAndRetry<ApiKey>((x, limit, pos) => x.GetApiKeys(new GetApiKeysRequest()
            {
                Limit = limit,
                Position = pos
            }));

            return results.ToDictionary(x => x.Id, x => new Key() {
                        Name = x.Name,
                        CreatedDate = x.CreatedDate,
                        Description = x.Description,
                        Enabled = x.Enabled,
                        Stages = x.StageKeys
                    });
        }

        public string ExportAsSwagger(string exportApiId, string stage)
        {
            var result = gateway.GetExport(new GetExportRequest()
            {
                RestApiId = exportApiId,
                ExportType = "swagger",
                Accepts = "application/json",
                StageName = stage
            });

            var sr = new StreamReader(result.Body);
            return sr.ReadToEnd();
        }
        
        private Resource GetRootResource(RestApi api)
        {
            foreach (var resource in gateway.BuildResourceList(api.Id))
            {
                if ("/".Equals(resource.Path))
                {
                    return resource;
                }
            }

            return null;
        }

        private RestApi GetApi(string apiId)
        {
            var api = gateway.WaitAndRetry(x => x.GetRestApi(new GetRestApiRequest() {RestApiId = apiId}));
            return new RestApi() { Id = api.Id, Name = api.Name };
        }
    }
}
    