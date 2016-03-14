using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.APIGateway;
using Amazon.APIGateway.Model;
using log4net;
using Newtonsoft.Json.Linq;

namespace Importer.Swagger.Aws.Impl
{
    public class ApiGatewaySdkMethodProvider : IApiGatewaySdkMethodProvider
    {
        private readonly IApiGatewaySdkModelProvider modelProvider;
        private readonly IApiGatewaySdkMethodResponseProvider methodResponseProvider;
        private readonly IApiGatewaySdkMethodParameterProvider methodParameterProvider;
        private readonly IApiGatewaySdkMethodIntegrationProvider methodIntegrationProvider;
        private readonly HashSet<string> processedModels;
        private readonly IAmazonAPIGateway gateway;
        protected ILog Log = LogManager.GetLogger(typeof(ApiGatewaySdkMethodProvider));

        public ApiGatewaySdkMethodProvider(HashSet<string> processedModels,
            IAmazonAPIGateway gateway,
            IApiGatewaySdkModelProvider modelProvider, 
            IApiGatewaySdkMethodResponseProvider methodResponseProvider, 
            IApiGatewaySdkMethodParameterProvider methodParameterProvider, 
            IApiGatewaySdkMethodIntegrationProvider methodIntegrationProvider)
        {
            this.processedModels = processedModels;
            this.gateway = gateway;
            this.modelProvider = modelProvider;
            this.methodResponseProvider = methodResponseProvider;
            this.methodParameterProvider = methodParameterProvider;
            this.methodIntegrationProvider = methodIntegrationProvider;
        }

        public void CreateMethods(RestApi api, SwaggerDocument swagger, Resource resource, PathItem path, IList<string> apiProduces)
        {
            var ops = GetOperations(path);

            ops.ForEach(x => {
                CreateMethod(api, swagger, resource, x.Key, x.Value, SwaggerHelper.GetProducesContentType(apiProduces, x.Value.Produces));
                Log.InfoFormat("Creating method for api id {0} and resource id {1} with method {2}", api.Id, resource.Id, x.Key);
            });
        }

        public void UpdateMethods(RestApi api, SwaggerDocument swagger)
        {
            foreach (var pathItem in swagger.Paths)
            {
                var fullPath = BuildResourcePath(swagger.BasePath, pathItem.Key);
                var path = pathItem.Value;

                var ops = GetOperations(path);

                foreach (var item in ops)
                {
                    var httpMethod = item.Key;
                    var op = item.Value;

                    // resolve the resource based on path - the resource is guaranteed to exist by this point
                    var resource = GetResource(api, fullPath);

                    var modelContentType = SwaggerHelper.GetProducesContentType(swagger.Produces, op.Produces);

                    if (gateway.DoesMethodExists(api.Id, httpMethod, resource.Id))
                    {
                        UpdateMethod(api, swagger, resource, httpMethod, op, modelContentType);
                    }
                    else {
                        CreateMethod(api, swagger, resource, httpMethod, op, modelContentType);
                    }
                }
            }
        }

        private void UpdateMethod(RestApi api, SwaggerDocument swagger, Resource resource, string httpMethod, Operation op, string modelContentType)
        {
            var operations = PatchOperationBuilder.With()
                .Operation(Operations.Replace, "/authorizationType", GetAuthorizationType(op))
                .Operation(Operations.Replace, "/apiKeyRequired", IsApiKeyRequired(swagger, op).ToString()).ToList();
            
            var result = gateway.UpdateMethod(new UpdateMethodRequest() {
                RestApiId = api.Id,
                HttpMethod = httpMethod,
                ResourceId = resource.Id,
                PatchOperations = operations
            });

            var method = new Method()
            {
                HttpMethod = result.HttpMethod,
                ApiKeyRequired = result.ApiKeyRequired,
                AuthorizationType = result.AuthorizationType,
                MethodIntegration = result.MethodIntegration,
                MethodResponses = result.MethodResponses,
                RequestModels = result.RequestModels,
                RequestParameters = result.RequestParameters
            };

            methodResponseProvider.UpdateMethodResponses(api, resource, method, swagger, modelContentType, op.Responses);
            methodParameterProvider.UpdateMethodParameters(api, resource, method, op.Parameters);
            methodIntegrationProvider.CreateIntegration(api, resource, method, op.VendorExtensions);
        }

        private void CreateMethod(RestApi api, SwaggerDocument swagger, Resource resource, string httpMethod, Operation op, string modelContentType)
        {
            var input = new PutMethodRequest
            {
                AuthorizationType = GetAuthorizationType(op),
                ApiKeyRequired = IsApiKeyRequired(swagger, op),
                RestApiId = api.Id,
                ResourceId = resource.Id
            };

            // set input model if present in body
            op.Parameters.Where(x => x.In.Equals("body")).ForEach(p => {
                //BodyParameter bodyParam = (BodyParameter)p;

                var inputModel = GetInputModel(p);

                input.RequestModels = new Dictionary<string, string>();

                // model already imported
                if (inputModel != null)
                {
                    processedModels.Add(inputModel);

                    Log.InfoFormat("Found input model reference {0}", inputModel);
                    input.RequestModels[modelContentType] = inputModel;
                }
                else
                {
                    // create new model from nested schema
                    string modelName = GenerateModelName(p);
                    Log.InfoFormat("Creating new model referenced from parameter: {0}", modelName);

                    if (p.Schema == null)
                    {
                        throw new ArgumentException("Body parameter '{0}' + must have a schema defined", p.Name);
                    }

                    modelProvider.CreateModel(api, modelName, p.Schema, swagger.Definitions, modelContentType);
                    input.RequestModels[modelContentType] = modelName;
                }
            });

            // create method
            input.HttpMethod = httpMethod.ToUpper();
            var result = gateway.PutMethod(input);

            var method = new Method()
            {
                HttpMethod = result.HttpMethod,
                ApiKeyRequired = result.ApiKeyRequired,
                AuthorizationType = result.AuthorizationType,
                MethodIntegration = result.MethodIntegration,
                MethodResponses = result.MethodResponses,
                RequestModels = result.RequestModels,
                RequestParameters = result.RequestParameters
            };

            methodResponseProvider.CreateMethodResponses(api, resource, method, swagger, modelContentType, op.Responses);
            methodParameterProvider.CreateMethodParameters(api, resource, method, op.Parameters);
            methodIntegrationProvider.CreateIntegration(api, resource, method, op.VendorExtensions);
        }

        private IDictionary<string, Operation> GetOperations(PathItem path)
        {
            IDictionary<string, Operation> ops = new Dictionary<string, Operation>();

            AddOp(ops, "get", path.Get);
            AddOp(ops, "post", path.Post);
            AddOp(ops, "put", path.Put);
            AddOp(ops, "delete", path.Delete);
            AddOp(ops, "options", path.Options);
            AddOp(ops, "patch", path.Patch);

            return ops;
        }

        private void AddOp(IDictionary<string, Operation> ops, string method, Operation operation)
        {
            if (operation != null)
            {
                ops[method] = operation;
            }
        }

        private string GetAuthorizationType(Operation op)
        {
            var authType = "NONE";

            if (op.VendorExtensions != null && op.VendorExtensions.ContainsKey(Constants.ExtensionAuth))
            {
                var vendorExtension = op.VendorExtensions[Constants.ExtensionAuth] as JObject;

                var authExtension = vendorExtension?.ToObject<Dictionary<string, string>>();

                if (authExtension != null)
                {
                    authType = authExtension["type"].ToUpper();
                }
            }

            return authType;
        }

        private bool IsApiKeyRequired(SwaggerDocument swagger, Operation op)
        {
            var apiKeySecurityDefinition = default(KeyValuePair<string, SecurityScheme>);

            if (swagger.SecurityDefinitions != null)
            {
                apiKeySecurityDefinition = swagger.SecurityDefinitions.FirstOrDefault(x => x.Value.Type.Equals("apiKey"));
            }

            if (apiKeySecurityDefinition.Equals(default(KeyValuePair<string, SecurityScheme>)))
            {
                return false;
            }

            var securityDefinitionName = apiKeySecurityDefinition.Key;

            if (op.Security != null)
            {
                return op.Security.Any(x => x.ContainsKey(securityDefinitionName));
            }

            if (swagger.Security != null)
            {
                return swagger.Security.Any(x => x.ContainsKey(securityDefinitionName));
            }

            return false;
        }


        private string GetInputModel(Parameter p)
        {
            var model = p.Schema;

            if (p.Schema.Ref != null)
                return p.Schema.Ref;

            //ToDo:What is this? based on input
            //if (model instanceof RefModel) {
            //    String modelName = ((RefModel)model).getSimpleRef();   // assumption: complex ref?
            //    return Optional.of(modelName);
            //}

            return string.Empty;
        }

        private string GenerateModelName(Parameter param)
        {
            return GenerateModelName(param.Description);
        }

        private string GenerateModelName(string description)
        {
            if (string.IsNullOrEmpty(description))
            {
                Log.Warn("No description found for model, will generate a unique model name");
                return "model" + Guid.NewGuid().ToString().Substring(0, 8);
            }

            // note: generating model name based on sanitized description
            return description.Replace(GetModelNameSanitizeRegex(), "");
        }

        private string GetModelNameSanitizeRegex()
        {
            return "[^A-Za-z0-9]";
        }

        private string BuildResourcePath(string basePath, string resourcePath)
        {
            if (basePath == null)
            {
                basePath = string.Empty;
            }
            var path = TrimSlashes(basePath);

            if (!string.IsNullOrEmpty(path))
            {
                path = "/" + path;
            }
            var result = (path + "/" + TrimSlashes(resourcePath)).TrimEnd('/');


            if (string.IsNullOrEmpty(result))
            {
                result = "/";
            }

            return result;
        }

        private string TrimSlashes(string str)
        {
            return str.TrimStart('/').TrimEnd('/');
        }

        private Resource GetResource(RestApi api, string fullPath)
        {
            foreach (Resource r in  BuildResourceList(api))
            {
                if (r.Path.Equals(fullPath))
                {
                    return r;
                }
            }

            return null;
        }

        private List<Resource> BuildResourceList(RestApi api)
        {
            var resourceList = new List<Resource>();

            var resources = gateway.GetResources(new GetResourcesRequest()
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