using System;
using System.Collections.Generic;
using Amazon.APIGateway;
using Amazon.APIGateway.Model;
using log4net;

namespace Importer.Swagger.Aws.Impl
{
    public class ApiGatewaySdkMethodResponseProvider : IApiGatewaySdkMethodResponseProvider
    {
        private readonly HashSet<string> processedModels;
        private readonly IAmazonAPIGateway gateway;
        private readonly IApiGatewaySdkModelProvider modelProvider;
        protected ILog Log = LogManager.GetLogger(typeof(ApiGatewaySdkMethodResponseProvider));

        public ApiGatewaySdkMethodResponseProvider(HashSet<string> processedModels, IAmazonAPIGateway gateway, IApiGatewaySdkModelProvider modelProvider)
        {
            this.processedModels = processedModels;
            this.gateway = gateway;
            this.modelProvider = modelProvider;
        }

        public void CreateMethodResponses(RestApi api, Resource resource, Method method, SwaggerDocument swagger, string modelContentType, IDictionary<string, Response> responses)
        {
            if (responses == null)
                return;
            
            // add responses from swagger
            responses.ForEach(x => {

                if (x.Key.Equals("default"))
                {
                    Log.Warn("Default response not supported, skipping");
                }
                else
                {
                    Log.InfoFormat("Creating method response for api {0} and method {1} and status {2}", api.Id, method.HttpMethod, x.Key);

                    var request = GetCreateResponseInput(api, swagger, resource, method, modelContentType, x.Value);
                    request.RestApiId = api.Id;
                    request.ResourceId = resource.Id;
                    request.StatusCode = x.Key;
                    request.HttpMethod = method.HttpMethod;

                    gateway.PutMethodResponse(request);
                }
            });
        }

        public void UpdateMethodResponses(RestApi api, Resource resource, Method method, SwaggerDocument swagger, string modelContentType, IDictionary<string, Response> responses)
        {
            foreach (var response in method.MethodResponses.Values)
            {
                gateway.DeleteMethodResponse(new DeleteMethodResponseRequest()
                {
                    RestApiId = api.Id,
                    ResourceId = resource.Id,
                    HttpMethod = method.HttpMethod,
                    StatusCode = response.StatusCode
                });
            }

            CreateMethodResponses(api, resource, method, swagger, modelContentType, responses);
        }

        private PutMethodResponseRequest GetCreateResponseInput(RestApi api, SwaggerDocument swagger, Resource resource, Method method, string modelContentType, Response response)
        {
            var input = new PutMethodResponseRequest();

            // add response headers
            if (response.Headers != null)
            {
                input.ResponseParameters = new Dictionary<string, bool>();
                response.Headers.ForEach(x =>
                {
                    input.ResponseParameters["method.response.header." + x.Key] = x.Value.Required;
                });
            }

            // if the schema references an existing model, use that model for the response
            var modelOpt = GetModel(api, resource, method, response);

            if (modelOpt != null)
            {
                input.ResponseModels = new Dictionary<string, string>();
                var modelName = modelOpt.Name;

                input.ResponseModels[modelContentType] = modelName;
                processedModels.Add(modelName);
                Log.InfoFormat("Found reference to existing model {0}", modelName);
            }
            else
            {
                // generate a model based on the schema if the model doesn't already exist
                if (response.Schema != null)
                {
                    var modelName = modelProvider.NameResolver.GetModelName(resource.PathPart, method.HttpMethod, response);

                    Log.InfoFormat("Creating new model referenced from response: {0}", modelName);

                    modelProvider.CreateModel(api, modelName, response.Schema, swagger.Definitions, modelContentType);
                    input.ResponseModels = new Dictionary<string, string> { [modelContentType] = modelName };
                }
            }

            return input;
        }

        private Model GetModel(RestApi api, Resource resource, Method method, Response response)
        {

            string modelName;

            // if the response references a proper model, look for a model matching the model name
            if (response.Schema?.Type != null && response.Schema.Type.Equals("ref"))
            {
                modelName = modelProvider.NameResolver.GetModelName(response.Schema.Ref);
            }
            else
            {
                // if the response has an embedded schema, look for a model matching the generated name
                modelName = modelProvider.NameResolver.GetModelName(resource.PathPart, method.HttpMethod, response);
            }

            try
            {
                var result = gateway.GetModel(new GetModelRequest() {RestApiId = api.Id, ModelName = modelName});
                return new Model()
                {
                    Id = result.Id,
                    Description = result.Description,
                    ContentType = result.ContentType,
                    Schema = result.Schema,
                    Name = result.Name
                };
            }
            catch (NotFoundException)
            {
                Log.InfoFormat("Cannot find model {0}", modelName);
            }
            
            return null;
        }
    }
}