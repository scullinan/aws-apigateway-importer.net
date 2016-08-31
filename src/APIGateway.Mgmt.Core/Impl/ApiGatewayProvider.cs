using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Amazon.APIGateway;
using Amazon.APIGateway.Model;
using APIGateway.Swagger;
using log4net;

namespace APIGateway.Management.Impl
{
    public class ApiGatewayProvider : IApiGatewayProvider
    {
        protected IAmazonAPIGateway Gateway;
        protected ILog Log = LogManager.GetLogger(typeof(ApiGatewayProvider));
        
        private readonly IApiGatewayModelProvider modelProvider;
        private readonly IApiGatewayDeploymentProvider deploymentProvider;
        private readonly IApiGatewayResourceProvider resourceProvider;
        private readonly IApiGatewayMethodProvider methodProvider;

        public ApiGatewayProvider(IAmazonAPIGateway gateway, IApiGatewayModelProvider modelProvider, IApiGatewayResourceProvider resourceProvider, 
            IApiGatewayMethodProvider methodProvider, IApiGatewayDeploymentProvider deploymentProvider)
        {
            this.Gateway = gateway;
            this.modelProvider = modelProvider;
            this.resourceProvider = resourceProvider;
            this.methodProvider = methodProvider;
            this.deploymentProvider = deploymentProvider;
        }

        public SwaggerDocument Get(string apiId)
        {
            throw new NotImplementedException();
        }

        public string Create(string apiName, SwaggerDocument swagger)
        {
            Log.InfoFormat("Creating API with Name {0}", apiName);

            var request = new CreateRestApiRequest
            {
                Name = !string.IsNullOrEmpty(swagger.Info.Title) ? swagger.Info.Title : apiName,
                Description = swagger.Info.Description
            };

            var response = Gateway.WaitAndRetry(x => x.CreateRestApi(request));

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

        public void Update(string apiId, SwaggerDocument swagger)
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

        public void Merge(string apiId, SwaggerDocument swagger)
        {
            Log.InfoFormat("Merging API {0}", apiId);

            var response = Gateway.WaitAndRetry(x => x.GetRestApi(new GetRestApiRequest() { RestApiId = apiId }));

            var api = response.RestApi();
            var rootResource = this.GetRootResource(api);

            modelProvider.UpdateModels(api, swagger);
            resourceProvider.UpdateResources(api, rootResource, swagger);
            methodProvider.UpdateMethods(api, swagger);
        }

        public void Delete(string apiId)
        {
            Log.InfoFormat("Deleting API {0}", apiId);

            Gateway.WaitAndRetry(x => x.DeleteRestApi(new DeleteRestApiRequest()
            {
                RestApiId = apiId
            }));
        }

        public void Destory(string apiId)
        {
            Log.InfoFormat("Wiping API {0}", apiId);

            var response = Gateway.WaitAndRetry(x => x.GetRestApi(new GetRestApiRequest() { RestApiId = apiId }));
            var api = response.RestApi();

            resourceProvider.DeleteResources(api);
            modelProvider.DeleteModels(api);
        }

        public void Deploy(string apiId, DeploymentDocument config)
        {
            Log.InfoFormat("Deploying API {0}", apiId);

            deploymentProvider.CreateDeployment(apiId, config);
        }

        public string Export(string apiId, string stageName, string exportType = "swagger", string accepts = "application/json")
        {
            var result = Gateway.GetExport(new GetExportRequest()
            {
                RestApiId = apiId,
                ExportType = exportType,
                Accepts = accepts,
                StageName = stageName
            });

            var sr = new StreamReader(result.Body);
            return sr.ReadToEnd();
        }

        public SwaggerDocument Combine(List<SwaggerDocument> documents)
        {
            var first = documents[0];

            foreach (var doc in documents.Skip(1))
            {
                foreach (var path in doc.Paths)
                {
                    UpdatePathItem(path.Value);
                    first.Paths.Add(path);
                }

                foreach (var def in doc.Definitions)
                {
                    if (!first.Definitions.ContainsKey(def.Key))
                        first.Definitions.Add(def);
                }
            }

            return first;
        }

        public string CreateApiKey(string apiId, string keyName, string stageName)
        {
            return deploymentProvider.CreateApiKey(apiId, keyName, stageName);
        }

        public void DeleteApiKey(string key)
        {
            deploymentProvider.DeleteApiKey(key);
        }

        public void ClearApiKeys()
        {
            deploymentProvider.ClearApiKeys();
        }

        public IDictionary<string, string> ListApis()
        {
            var results = Gateway.PageWaitAndRetry<RestApi>((x, limit, pos) => x.GetRestApis(new GetRestApisRequest() {
                Limit = limit,
                Position = pos
            }));

            return results.ToDictionary(x => x.Id, x => x.Name);
        }

        public IDictionary<string, Key> ListKeys()
        {
            var results = Gateway.PageWaitAndRetry<ApiKey>((x, limit, pos) => x.GetApiKeys(new GetApiKeysRequest()
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

        public IEnumerable<string> ListOperations(string apiId)
        {
            throw new NotImplementedException();
        }
        
        private Resource GetRootResource(RestApi api)
        {
            foreach (var resource in Gateway.BuildResourceList(api.Id))
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
            var api = Gateway.WaitAndRetry(x => x.GetRestApi(new GetRestApiRequest() {RestApiId = apiId}));
            return new RestApi() { Id = api.Id, Name = api.Name };
        }

        private void UpdatePathItem(PathItem pathItem)
        {
            UpdateDescription(pathItem.Get);
            UpdateDescription(pathItem.Post);
            UpdateDescription(pathItem.Put);
            UpdateDescription(pathItem.Delete);
            UpdateDescription(pathItem.Options);
            UpdateDescription(pathItem.Patch);
            UpdateDescription(pathItem.Head);
        }

        private void UpdateDescription(Operation op)
        {
            if (op == null) return;

            op.Description = op.Summary;
            op.Summary = null;
        }
    }
}
    