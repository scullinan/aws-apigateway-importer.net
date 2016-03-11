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
            {
                return;
            }

            // add responses from swagger
            responses.ForEach(x => {

                if (x.Key.Equals("default"))
                {
                    Log.Warn("Default response not supported, skipping");
                }
                else
                {
                    Log.InfoFormat("Creating method response for api {0} and method {1} and status {2}", api.Id, method.HttpMethod, x.Key);

                    var request = GetCreateResponseInput(api, swagger, modelContentType, x.Value);
                    request.RestApiId = api.Id;
                    request.ResourceId = resource.Id;
                    request.StatusCode = x.Key;
                    request.HttpMethod = method.HttpMethod;

                    gateway.PutMethodResponse(request);
                }
            });
        }

        private PutMethodResponseRequest GetCreateResponseInput(RestApi api, SwaggerDocument swagger, string modelContentType, Response response)
        {

            PutMethodResponseRequest input = new PutMethodResponseRequest();

            // add response headers
            if (response.Headers != null)
            {
                input.ResponseParameters = new Dictionary<string, bool>();
                response.Headers.ForEach(x =>
                {
                    input.ResponseParameters["method.response.header." + x.Key] = x.Value.Required;
                }); //ToDo Required?
            }

            // if the schema references an existing model, use that model for the response
            Model modelOpt = GetModel(api, response);
            if (modelOpt != null)
            {
                input.ResponseModels = new Dictionary<string, string>();
                var modelName = modelOpt.Name;

                input.ResponseModels[modelContentType] = modelName;
                this.processedModels.Add(modelName);
                Log.InfoFormat("Found reference to existing model {0}", modelName);
            }
            else
            {
                // generate a model based on the schema if the model doesn't already exist
                if (response.Schema != null)
                {
                    string modelName = GenerateModelName(response);

                    Log.InfoFormat("Creating new model referenced from response: {0}", modelName);

                    modelProvider.CreateModel(api, modelName, response.Schema, swagger.Definitions, modelContentType);

                    input.ResponseModels = new Dictionary<string, string> { [modelContentType] = modelName };
                }
            }

            return input;
        }

        string GenerateModelName(Response response)
        {
            return GenerateModelName(response.Description);
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

        private Model GetModel(RestApi api, Response response)
        {

            string modelName;

            // if the response references a proper model, look for a model matching the model name
            if (response.Schema?.Type != null && response.Schema.Type.Equals("ref"))
            {
                modelName = response.Schema.Ref;
            }
            else
            {
                // if the response has an embedded schema, look for a model matching the generated name
                modelName = GenerateModelName(response);
            }

            try
            {
                var result = gateway.GetModel(new GetModelRequest() { RestApiId = api.Id, ModelName = modelName });
                return new Model()
                {
                    Description = result.Description,
                    ContentType = result.ContentType,

                };
            }
            catch (Exception ignored) { }

            return null;
        }
    }
}